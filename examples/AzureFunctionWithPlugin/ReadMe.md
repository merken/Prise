## Azure Functions Example

This directory contains an example of loading a Plugin **over the network** when an Azure Function is executed: **PluginFunction**.
An Azure Function with dependencies (Prise) requires an **[FunctionsStartup]** factory, similar to a **[PluginBootstrapper]** in Prise.
This startup file will bootstrap all the services required for the Function it is targeting, it is here that we AddPrise, via **AddPriseWithPluginLoader**.

During a function execution, we have no way to read the request before it hits the actual function, so we need a custom **PluginLoader** that we can feed the name of the plugin after it has been configured. Typically, this is done at launch-time.

The **FunctionPluginLoader** basically overrides the each method of the Prise PluginLoader and reads the modified componentName as assemblyName.
This allows you to set the plugin to load after the instantiation of the FunctionPluginLoader and load the plugin afterwards.
Since no plugins are deployed together with the Azure Function, the loader will get the plugin assembly over the network, from the PluginServer. On which, all the plugins are deployed and hosted.

### TOO LONG DID NOT READ
Launch the PluginServer using the `dotnet run --urls=https://localhost:5003` command from inside the **PluginServer** directory.
Publish all the components using the `cake` command from inside the **Plugins** directory.

Launch the PluginFunction using the `func start --build` command from inside the **Plugin.Function** directory.

Try out the Azure Function by navigating to `http://localhost:7071/api/ComponentFunction?&component=hello.plugin&input=Maarten`