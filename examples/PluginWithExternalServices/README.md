## Loading a plugin with dependencies

Plugins are activated using their *default parameterless constructor*. If a plugin requires dependencies to be injected, we will prepare an empty *ServiceCollection* that you can fill up with your required services. This is done via a *PluginBootstrapper*.

When your plugin requires dependencies to be injected via *Constructor Dependency Injection*, a *PluginBootstrapper* is required.
The goal of Prise is to provide an *easy setup* alongside *backwards compatibility*. By using a *Factory Method*, the creation of the plugin resides *within the plugin itself*, contained in the assembly. In other words, this should never fail. If we take the responsibility of creating the plugin outside of this scope, into the *PluginLoader*, and thus, the *host application*. Then this application becomes responsible of looking for the best fitting contructor to activate. The host would be responsible of *serializing the incoming constructor dependencies* and it would break the contract eventually. Requiring you to upgrade each plugin you still wish to support.

A *Factory Method* solves this issue, it only depends on the *System.IServiceProvider* interface in order to construct an instance of the plugin.

To recap, three things are required in order to have your plugin dependencies injected:
- A constructor containing your dependencies for your plugin  `public MyPlugin(IDependencyForPlugin dependency)` or `internal MyPlugin(IDependencyForPlugin dependency)` or  `protected MyPlugin(IDependencyForPlugin dependency)`
- A *static* Factory Method inside the plugin to construct your plugin instance `public static MyPlugin CreateMyPlugin(IServiceProvider serviceProvider)`
- A *PluginBootstrapper* dedicated to your plugin type `[PluginBootstrapper(PluginType = typeof(MyPlugin))]`

This example relies on concepts from a previous example, please check out the *PluginFromNetwork* for reference.

The MyHost application is an ASP.NET Core application that will load plugins from a Plugins directory *from disk at runtime*.
When the *HelloController* is called, a *IHelloPlugin* is loaded based on the *Accept-Language* HTTP header. If this an unknown header, the *RandomPlugin* will be loaded.

This plugin has a dependency on the Random.Domain library, no other plugin shares this dependency.
The *IRandomService* needs to be injected for the RandomPlugin to work, so we need a :
- Constructor containing the dependency `protected RandomPlugin(IRandomService service)`
- Factory Method to construct an instance of RandomPlugin `public static RandomPlugin ThisIsTheFactoryMethod(IServiceProvider serviceProvider)`
- And a bootstrapper for our RandomPlugin `[PluginBootstrapper(PluginType = typeof(RandomPlugin))]`

All other plugins and code copies from the *PluginFromNetwork* example and are not modified, apart from;
the MyHost application is modified to load plugins from disk using the *LocalDiskAssemblyLoader<T>*, plus both the *LanguageBasedAssemblyNameProvider* and *LanguageBasedPluginLoadOptions* were modified to provide the correct values to load the plugins based on the Accept-Language HTTP header.

### Starting the MyHost application
You can start the application using the `dotnet run --urls=https://localhost:5003` command from within the MyHost directory.
Since no plugins were copied into the running MyHost application directory, nothing will work, yet.

Copy over the plugins using the `cake` command from inside the Plugins directory. This will build, publish and copy all the `.Plugin` directories to the MyHost/bin/Debug/netcoreapp3.0/Plugins directory.

If you use Postman and provide a nonsense value for the Accept-Header, you will see the effect of the RandomPlugin.

### Lazy loading
Off course, lazy loading is also supported via this setup, please see the previous example for reference: PluginFromNetwork;

### TOO LONG DID NOT READ
Launch the MyHost application using the `dotnet run --urls=https://localhost:5003` command from inside the MyHost directory.

Publish all the components using the `cake` command from inside the Plugins directory.

Open your browser to https://localhost:5003/hello?input=Maarten or use Postman to change the Accept-Header in order to see the results.