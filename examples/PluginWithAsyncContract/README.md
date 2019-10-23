## Version 1.2.0 introduces support for plugin contracts that return Tasks

Up until version 1.1.x, returning a Task<T> from your contract would return in an error. Version **1.2.0** introduces support for this requirement.

In this example, you will find an **IHelloWorldPlugin** inside the Contract directory that exposes two task-based contracts. `Task<string> SayHelloAsync(string language, string input);` and `Task<HelloDictionary> GetHelloDictionaryAsync();`.

The goal of the host is to try to say hello based on an input (name) and a language. If the language is not supported, the plugin will throw an exception. This can be caught in the Host, which will then get all the supported languages from the plugin and display that as a result.

In order to make this work, the **PluginProxy** needed to return an empty **Task<T>**, of which the result will come later. So we internally subscribe to the Task<T> from the plugin and set the result whenever that comes back. The problem of **DispatchProxy** is that it **does not support an Async interface**. So we need to do the plumbing ourselves. Since the DispatchProxy does not allow for strong typing, and there's **no non-generic TaskCompletionSource in the standard libary**, we were forced to build one ourselves. This **TaskCompletionSource** allows to be constructed with a **Type parameter** and will return an object as result.

In the **BinaryFormatResultConverter** we'll return a non-generic Task, coming from a non-generic TaskCompletionSource and resolve the value once it arrives from the plugin, serialization is done as usual. Always keep in mind that your return types must be attributed with `[Serializable]` in order for **deserialization to work!** This is, at this point, the **major drawback** of the Prise framework.

With this new feature in place, we can start creating Contracts that return Tasks!

### Run the example
The **HelloWorldPlugin** will load a file from disk asynchronously, we assume the plugin resides inside the Plugins directory on the host, this is were we will look for the **Dictionary.json**
`{
    "en": "Hello",
    "nl": "Hallo",
    "fr": "Bonjour"
}`

As you can see, providing `en, nl or fr` as a language will work, but `de` won't.

Now, we will test this using our the **MyHost** application.

You can start the application using the `dotnet run --urls=https://localhost:5003` command from within the MyHost directory.
Since no plugins were copied into the running MyHost application directory, nothing will work, yet.

Copy over the plugins using the `cake` command from inside the HelloWorldPlugin directory. This will build, publish and copy the output from the HelloWorldPlugin directory to the MyHost/bin/Debug/netcoreapp3.0/Plugins directory.

If you use Postman and GET the following URL : https://localhost:5003/hello?&input=Maarten&language=en

HTML Result:
`Hello Maarten`

To trigger the exception handling, GET the following URL : https://localhost:5003/hello?&input=Maarten&language=de

HTML Result:
`Language de is not supported. Supported languages are : en,nl,fr`

### TOO LONG DID NOT READ
Launch the MyHost application using the `dotnet run --urls=https://localhost:5003` command from inside the MyHost directory.

Publish the HelloWorldPlugin using the `cake` command from inside the Plugins directory.

Open your browser to the following URL : https://localhost:5003/hello?&input=Maarten&language=en

HTML Result:
`Hello Maarten`

Open your browser to the following URL : https://localhost:5003/hello?&input=Maarten&language=de

HTML Result:
`Language de is not supported. Supported languages are : en,nl,fr`