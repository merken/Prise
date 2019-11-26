## Loading plugins from over the network

Prise provides a *NetworkAssemblyLoader* out of the box. This loader uses the *IHttpClientFactory* to get an *HttpClient* in order to *GET* a specific assembly via an *HTTP* network call.

We've made sure that only *minimal effort* is required in order to configure the out of box *NetworkAssemblyLoader* to fit your needs.
There are two parts in the loading process.
- Loading the plugin assembly by name
- Loading all dependecies from that assembly (discovery)

You can influence *both* loading paths using custom code.

> Keep in mind that we assume you `dotnet publish` your assemblies to a dedicated plugin directory on a remote location.
> All direct dependecies should reside in the same folder as the plugin, by default.

The PluginServer project acts as remote network location for our plugins, all plugins are hosted under the Plugins directory.
To start the server, `dotnet run` from inside the PluginServer directory. You should be able to browse the directory in your browser on https://localhost:5001/Plugins

To populate this directory with the compiled plugins, run the `cake` command from within the Plugins directory.
This will build, publish and copy all the `.Plugin` directories to the PluginServer Plugins directory.

### Starting the MyHost application
The MyHost application uses the WithNetworkAssemblyLoader method in order to register the IHelloPlugin. The following files will provide a custom path to load these plugins:
- LanguageBasedPluginLoadOptions
- LanguageBasedAssemblyNameProvider

Based on the HTTP Accept-Language header from the current context, a different plugin will be loaded.
en-US ==> Hello.Plugin.dll
fr-FR ==> Bonjour.Plugin.dll
nl-BE ==> Hallo.Plugin.dll

It would suffice if both these plugins would reside inside the same directory on the PluginServer, but each plugin has its own dedicated directory.
So we need to provide an LanguageBasedAssemblyNameProvider in order to load the correct assembly.
Also, we need to provide custom INetworkAssemblyLoaderOptions to point the initial path to the assembly, LanguageBasedPluginLoadOptions.

The LanguageBasedPluginLoadOptions will also allow us to load the dependencies from that plugin from that path.

### Lazy loading
By default, the IHelloPlugin will be registered in the ServiceCollection of the host service. By resolving this plugin, an IPluginLoader<T> will load the plugin when the IHelloPlugin is requested. This is why you're able to inject a IHelloPlugin into your HelloController.

If you want to have more control over the loading of the plugin, you can inject an *IPluginLoader<IHelloPlugin>* and `.Load()` the plugin on demand. This is called lazy loading. Please check out the *LazyHelloController* for an example of this.


### TOO LONG DID NOT READ
Launch the PluginServer using the `dotnet run --urls=https://localhost:5003` command from inside the PluginServer directory.

Publish all the components using the `cake` command from inside the Plugins directory.

Open your browser to https://localhost:5001/Plugins to see al the plugins.

Launch the MyHost application using the `dotnet run --urls=https://localhost:5001` command from inside the MyHost directory.

Open your browser to https://localhost:5003/hello?input=Maarten or use Postman to change the Accept-Header in order to see the results.