# Prise
A .NET Core Plugin Frawework!

## 1.5.0
This release has focused on allowing Prise to load MVC plugins using the Prise.Mvc package.

Improvements were done in terms of sharing services between the host and the plugin.
There are three types of services that can be consumed inside the plugin:
- Host service
- Local service (plugin service)
- Shared service

### Host service
A host service is a service of which its type definition exists in both the host and the plugin (IConfiguration, Microsoft.Extensions.Configuration.Abstractions). Its implementation type also resides inside the host. The host is responsible of providing the instance to this type. Thus, when loading this service. The plugin will load this instance against its representation of it.
```
 // Registers the plugin that will be loaded via scanning
services.AddPrise<ITranslationPlugin>(options =>
        options
        .WithDefaultOptions(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"))
        .ScanForAssemblies(composer =>
            composer.UseDiscovery())
        .WithHostFrameworkProvider<AppHostFrameworkProvider>()
        .ConfigureHostServices(hostServices =>
        {
            // These services are registered as host types
            // Their types and instances will be loaded from the MyHost
            hostServices.AddHttpContextAccessor();
            hostServices.AddSingleton<IConfiguration>(Configuration); // => this is the Host service
        })
    );
```
You can resolve this service by calling ```IPluginServiceProvider.GetHostService<TService>()```
Using a host service breaks backwards compatability, since it requires the host and the plugin to have the exact reference to the service type definition:
```
<ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="3.0.0" />
</ItemGroup>
```
Any mismatch in package version will result in the following exception:
```throw new PrisePluginException($"Could not resolve Host Service <{typeof(T).Name}> Could there be a version mismatch?");```

It is advised to move towards a **Shared Service** using a **PluginBridge**.

### Local service (plugin service)
This is a service that is only known to the plugin, its type definition and implementation lives inside the plugin or one of its dependencies. It is registered using the PluginBootstrapper.
```
[PluginBootstrapper(PluginType = typeof(TranslationPlugin))]
public class TranslationPluginBootstrapper : IPluginBootstrapper
{
    public IServiceCollection Bootstrap(IServiceCollection services)
    {
        services.AddScoped<IDictionaryService, DictionaryService>(); // ==> Local Service
        return services;
    }
}
```
You can resolve this service by calling ```IPluginServiceProvider.GetPluginService<TService>()```

### Shared service
A shared service, is a service of which its type is defined in an external library that is shared between the Host and the Plugin.
Similar to the Contact, but another library entirely. This allows for the plugin to call into the Host in order to do logic.
Essentially, it is the reverse of what **PriseProxy** achieves.

To invoke this service, the **PluginBridge** Nuget package is required.
For now, this requires additional work from the plugin side, you will need to create a bridge service of the type you wish to invoke in the host. This will improve going forward, for now, it is what it is.

A Shared service type could be the following:
```
public class CurrentLanguage
{
    public string LanguageCultureCode { get; set; }
}

public interface ICurrentLanguageProvider
{
    Task<CurrentLanguage> GetCurrentLanguage();
}
```

The host would implement this like so:
```
public class AcceptHeaderLanguageProvider : ICurrentLanguageProvider
{
    private readonly IHttpContextAccessor httpContextAccessor;
    public AcceptHeaderLanguageProvider(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    // Get the language based on the current HTTP Headers
    public async Task<CurrentLanguage> GetCurrentLanguage()
    {
        return new CurrentLanguage 
        { LanguageCultureCode = this.httpContextAccessor.HttpContext.Request.Headers["Accept-Language"][0] };
    }
}
```

The shared service is then registered in the AddPrise config:
```
// Registers the plugin that will be loaded via scanning
.AddPrise<ITranslationPlugin>(options =>
        options
        .WithDefaultOptions(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"))
        .ScanForAssemblies(composer =>
            composer.UseDiscovery())
        .ConfigureSharedServices(sharedServices =>
        {
            // The AcceptHeaderlanguageService is known in the Host, but the type is registered as a remote type
            // This encourages backwards compatability
            sharedServices.AddTransient<ICurrentLanguageProvider, AcceptHeaderLanguageProvider>();
        })
    );
```
This service cal be resolved in the plugin by using the **IPluginServicesProvider** inside the **PluginFactory**:
```
[PluginFactory]
public static TranslationPlugin ThisIsTheFactoryMethod(IPluginServiceProvider services)
{
    var hostService = services.GetSharedHostService(typeof(ICurrentLanguageProvider));
    var hostServiceBridge = new CurrentLanguageProviderBridge(hostService);

    return new TranslationPlugin(
        hostServiceBridge // This service is provided by the Prise.IntegrationTestsHost application and is registered as a Remote type
    );
}
```

The PluginBridge package allows you to **proxy** into the host. (reverse PriseProxy).
```
public class CurrentLanguageProviderBridge : ICurrentLanguageProvider
{
    private readonly object hostService;
    public CurrentLanguageProviderBridge(object hostService) // instance of the host AcceptHeaderLanguageProvider
    {
        this.hostService = hostService;
    }

    public Task<CurrentLanguage> GetCurrentLanguage()
    {
        var methodInfo = typeof(ICurrentLanguageProvider).GetMethod(nameof(GetCurrentLanguage));
        return PrisePluginBridge.Invoke(this.hostService, methodInfo) as Task<CurrentLanguage>; // Reflection invoke
    }
}
```

*Miscellanious*
- Logging, Prise now has a default **NullPluginLogger**, **ConsolePluginLogger<T>** and a **PluginLoggerBase<T>** that outputs logs to the console.
- Tests, Tests, Tests. Prise now has lots of Integration Tests, with more Unit Tests to come
- Prise.Proxy is now its own Nuget Package!
- Prise.PluginBridge was introduced!
- Prise.Mvc is a new feature that allows you to load Mvc ApiControllers and Views from a plugin!
- Improvements in terms of logging, better exception throwing and general code cleanup

*New APIs:*
- **LogToConsole** was added to the **PluginLoadOptionsBuilder** which will log to the Console.
- **IPluginServiceProvider** can be injected in a [PluginFactory] instead of the **IServiceProvider**.
- **ConfigureHostServices** was added to the **PluginLoadOptionsBuilder**
- **AddPriseControllersAsPlugins** was added to the **PluginLoadOptionsBuilder** via the Prise.Mvc package
- **AddPriseRazorPlugins** was added to the **PluginLoadOptionsBuilder** via the Prise.Mvc package

## 1.4.0
Introduction of Assembly Discovery via the Prise.AssemblyScanning.Discovery package.

## 1.3.3

*New APIs:*
- WithSelector<T> API was added to inject your own custom PluginTypeSelector
  

## 1.3.2
**Contains breaking changes**

A plugin that has no additional setup required, should not contain an IPluginBootstrapper.
The Factory method will be passed a configured IServiceCollection that is constructed using the **ConfigureSharedServices** option.

*New features:*
- Removed the requirement for a **IPluginBootstrapper** when using a **PluginFactoryAttribute**

*Breaking changes:*
- **NetCoreActivator** class was replaced by **DefaultRemotePluginActivator**

Please see https://github.com/merken/Prise/tree/master/examples/PluginWithExternalServices for an example.

## 1.3.1

*New features:*
- Unloading of assemblies using a WeakReference : https://docs.microsoft.com/en-us/dotnet/standard/assembly/unloadability#use-collectible-assemblyloadcontext

## 1.3.0
**Contains breaking changes**
**Prise.Infrastructure package has been deprecated**

Overhaul of Prise, the Prise.Infrastructure package has been deprecated, please use the **Prise.Plugin** package.
This package was stripped from its dependencies on anything other than the required for the *PluginAttribute*, *PluginBootstrapperAttribute*, *PluginFactoryAttribute* and *IPluginBootstrapper* interface.

*Changes in namespaces are:*
- Prise.Infrastructure.NetCore assembly, Prise.Infrastructure namespace ==> -no change-
- Prise.Infrastructure.NetCore assembly, Prise.Infrastructure.NetCore namespace ==> **Prise**
- Prise.Infrastructure assembly, Prise.Infrastructure namespace ==> **Prise.Plugin**

*New APIs:*
- PluginSelector, select a specific plugin from a list of loaded plugin types (when using a plugin assembly with multiple plugins)
- IgnorePlatformInconsistencies, allows you to ignore platform (netstandard, netcoreapp) inconsistencies between plugin and host

*New features:*
- Broader support for more complex plugins, see MicroserviceWithPlugins example for more details
- Support for Assembly unloading (in .NET Core 3.0)
- Improved support for loading plugins over the network by saving the required assemblies on local disk
- Major unification of the .NET Core 2.1 and .NET Core 3.0 codebase
- Lots of bugfixes
  
## 1.2.4

Bugfixes

## 1.2.3

*New features:*
- Fixed bugs when loading assemblies
- Added support to specify dependency loading preference
- Added support for async Contract, Contracts that return Task<T>
- Unloading the loaded plugins using scoping and IDisposable
- Added fully working examples
- Targeting .NET Core 3.0
- Removed the requirement for the [Serializable] attribute on return types
- Using Newtonsoft.JSON for serialization in .NET Core 2.1
- Using System.Text.Json for serialization in .NET Core 3.0
- Remove dependency bulk in .NET Core 3.0 package
- Introduction of a non-generic TaskCompletionSource
- Added Integration Tests
- Updated docs

## 1.2.1
Further unification of .NET Core 2.1 and .NET Core 3.0 codebase

## 1.2.0
Introducing support for Async contracts by a non-generic TaskCompletionSource

## 1.1.0
Bugfixes and more code unification

## 1.0.5

Bugfixes

## 1.0.3

Bugfixes

## 1.0.2

Bugfixes

## 1.0.1

Bugfixes

## 1.0.0
Initial release of Prise
