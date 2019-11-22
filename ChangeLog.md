# Prise
A .NET Core Plugin Frawework!

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
