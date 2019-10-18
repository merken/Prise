## Getting Started

It's not exacly a three-step solution, but it's not that complicated either.

First, create a new classlibrary project this will be known as 'the contract'
- Create an interface (the contract)

Secondly, create a new classlibary project for our first plugin.
- Create a class that implements this interface (the plugin)
- Add a reference to the contract project
- Install Prise.Infrastructure `dotnet add package Prise.Infrastructure`
- Add the `[Plugin]` attribute to the plugin, mark it with the PluginType of the contract
- Build and publish the assembly, you've now created your first pluggable plugin!

Third, create a host project, for example an ASP.NET Core application.
- Install Prise `dotnet add package Prise`
- Hook up Prise using the contract
```
services.AddPrise<name of the contract>(options =>
    options
        .WithPluginAssemblyName("<name of the plugin assembly>.dll")
```
- Use the contract in a controller or service, this will be injected!
- Build and run the host

Almost there, we have our host ready to hot-load our plugin when it is requested.
Whilst the host is running, add a `Plugins` directory inside the bin/netcoreapp3.0 directory.
Copy the plugin's published files into this folder.

When the controller is consumed, the plugin will be loaded on demand and executed.

Check the example projects!

Getting started example: https://github.com/merken/Prise/tree/master/examples/SimplePlugin

Example using all possible features of Prise: https://github.com/merken/Prise/tree/master/examples/ComplexPlugin
