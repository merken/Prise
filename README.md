# What is Prise?

<img src="https://github.com/merken/Prise/blob/master/docs/prise.png?raw=true" 
alt="Prise Logo" width="65" height="65" style="float:left; padding-right:15px;" />

Prise is a plugin framework for .NET Core applications, written in .NET Core.
The goal of Prise, is enable you to write **decoupled pieces** of code using the least amount of effort, whilst maximizing the customizability. Prise helps you load plugins from **foreign assemblies**. It is built to decouple the **local** and **remote** dependencies, and strives to avoid assembly mismatches.

Prise features:
- Easy setup
- Eager and Lazy loading of plugins
- Fully customizable loading of plugins
- Loading plugins with their own dependencies
- Loading plugins over the network
- Supporting backwards compatibility for older plugins

## Builds and Tests
![alt text](https://github.com/merken/prise/workflows/Prise%20Build%20.net%20core%202/badge.svg "Prise Build .NET CORE 2")

![alt text](https://github.com/merken/prise/workflows/Prise%20Build%20.net%20core%203/badge.svg "Prise Build .NET CORE 3")

![alt text](https://github.com/merken/prise/workflows/Prise%20unit%20tests/badge.svg "Prise Unit Tests")

![alt text](https://github.com/merken/prise/workflows/Prise%20integration%20tests%20.net%20core%202/badge.svg "")

![alt text](https://github.com/merken/prise/workflows/Prise%20integration%20tests%20.net%20core%203/badge.svg "")

![alt text](https://github.com/merken/prise/workflows/Prise%20backwards%20compatibility%20tests%20.net%20core%202%20on%203/badge.svg "")

![alt text](https://github.com/merken/prise/workflows/Prise%20backwards%20compatibility%20tests%20.net%20core%203/badge.svg "")

![alt text](https://github.com/merken/prise/workflows/Prise%20backwards%20compatibility%20.net%20core%202/badge.svg "")

## Latest version
![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Prise) [Prise](https://www.nuget.org/packages/Prise) 

![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Prise.Mvc) [Prise.Mvc](https://www.nuget.org/packages/Prise.Mvc)

![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Prise.Proxy) [Prise.Proxy](https://www.nuget.org/packages/Prise.Proxy) 

![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Prise.Plugin) [Prise.Plugin](https://www.nuget.org/packages/Prise.Plugin) 

![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Prise.PluginBridge) [Prise.PluginBridge](https://www.nuget.org/packages/Prise.PluginBridge) 

![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Prise.AssemblyScanning.Discovery) [Prise.AssemblyScanning.Discovery](https://www.nuget.org/packages/Prise.AssemblyScanning.Discovery)


# How does Prise work ?
At the heart, Prise uses a **DispatchProxy** of your contract. Every call to a method channels through this proxy and is delegated to the loaded plugin.

Prise **does not require the plugin to implement the interface** provided by the contract, allowing you to extend the contract going forward, whilst still keeping your **old plugins backwards compatible**.
You can customize Prise to fit your needs, examples of this are:
- Loading plugins from over the network
- Loading plugins based on a condition at runtime (Environment Variable)
- Loading multiple plugins and choosing which one fits the purpose for a specific task
- Loading lazily or eagerly



Please see the [getting started](https://github.com/merken/Prise/blob/master/GettingStarted.md) to check out the various example projects.
