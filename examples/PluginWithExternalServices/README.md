## Injecting a plugin with external dependencies

Sometimes, you want a plugin to reach out back into the host application in order to get data from a service that is configured in the host.

This can be done by providing a shared contract (`IExternalService`) to the plugin, it needs to reside in the Contract assembly.

The host application will construct an instance of this service based on its context. This service will be passed through the **PluginFactory** method in order to construct the plugin.

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();
    services.AddHttpContextAccessor(); // Required for the AcceptHeaderlanguageService
    // IExternalService resides in the Contract assembly
    // When we resolve a IExternalService in the host, the AcceptHeaderlanguageService will be constructed
    services.AddScoped<IExternalService, AcceptHeaderlanguageService>();

    services.AddPrise<IHelloPlugin>(options => options
        .WithLocalDiskAssemblyLoader("Plugins\\LanguageBased.Plugin")
        .WithPluginAssemblyName("LanguageBased.Plugin.dll")
        // The plugin is a netstandard library, the host is a netcoreapp, ignore this inconsistency
        .IgnorePlatformInconsistencies() 
        .ConfigureSharedServices(sharedServices =>
        {
            // creates a IServiceProvider in the host
            var myHostServiceProvider = services.BuildServiceProvider();
            // Gets an instance of the AcceptHeaderlanguageService
            var acceptheaderLanguageService = myHostServiceProvider.GetRequiredService<IExternalService>();
            // Adds it to the ServiceCollection of the plugin for creating the LanguageBasedPlugin using the factory method
            // This can be added as a Singleton, since the scope is defined by the MyHost, and this is always Scoped.
            sharedServices.AddSingleton<IExternalService>(acceptheaderLanguageService);
        })
    );
}
```

### Starting the MyHost application
You can start the application using the `dotnet run --urls=https://localhost:5003` command from within the MyHost directory.
Since no plugins were copied into the running MyHost application directory, nothing will work, yet.

Copy over the plugins using the `cake` command from inside the Plugins directory. This will build, publish and copy all the `.Plugin` directories to the MyHost/bin/Debug/netcoreapp3.0/Plugins directory.

If you use Postman and provide a nonsense value for the **Accept-Header**, you will see the effect of the RandomPlugin.

### TOO LONG DID NOT READ
Launch the MyHost application using the `dotnet run --urls=https://localhost:5003` command from inside the MyHost directory.

Publish all the components using the `cake` command from inside the Plugins directory.

Open your browser to https://localhost:5003/hello?input=Maarten or use Postman to change the **Accept-Header** in order to see the results.