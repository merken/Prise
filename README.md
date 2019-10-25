## What is Prise?

Prise is a plugin framework for .NET Core applications, written in .NET Core.
The goal of Prise, is enable you to write decoupled pieces of code using the least amount of effort, whilst maximizing the customizability. Prise helps you load plugins from foreign assemblies. It is built to decouple the local and remote dependencies, and strives to avoid assembly mismatches.

Prise features:
- Fully customizable loading of plugins
- Eager and Lazy loading of plugins
- Loading plugins with their own dependency injection container

## Latest version
[Prise](https://www.nuget.org/packages/Prise) ![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Prise)

[Prise.Infrastructure](https://www.nuget.org/packages/Prise.Infrastructure) ![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Prise.Infrastructure)


# How does Prise work ?
At the heart, Prise connects the host with a plugin by using a DispatchProxy, all method calls are channeled through to the loaded plugin.
Prise does not require the plugin to implement the interface provided by the contract, allowing you to extend the contract going forward, whilst still keeping your old plugins backwards compatible.
You can customize Prise to fit your needs, examples of this are:
- Loading plugins from over the network
- Loading plugins based on a condition at runtime (Environment Variable)
- Loading multiple plugins and choosing which one fits the purpose for a specific task
- Loading lazily or eagerly (default)

Out of the box, Prise supports eager loading of plugins from the Plugins directory from the location of the executing assembly (RootPath).

Please check out the various examples or view the Get-Started page to help you on your way.

<img src="https://github.com/merken/Prise/blob/master/docs/prise.png?raw=true" 
alt="Prise Logo" width="240" height="240" border="10" style="left:auto;right:auto;" />
