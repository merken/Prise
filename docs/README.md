TODO
<pre>Add Default Loader
Add What is a Prise Plugin Package
Add how to write your own loader
Add How to structure your Plugin directories
Add what has changed
- PluginBridge=> ReverseProxy
- Added lazy factories to remove the need to hook Prise into the ServiceCollection
- Improved the load method
- UnitTests
</pre>
<p align="center">
  <a href="" rel="noopener">
 <img width=150px height=150px src="prise.png" alt="Project logo"></a>
</p>

<h3 align="center">Prise</h3>

<div align="center">

  [![Status](https://img.shields.io/badge/status-active-success.svg)]() 
  [![GitHub Issues](https://img.shields.io/github/issues/merken/prise?style=flat-square)](https://github.com/merken/prise/issues)
  [![GitHub Pull Requests](https://img.shields.io/github/issues-pr/merken/prise?style=flat-square)](https://github.com/merken/prise/pulls)
  [![Prise Nuget Version](https://img.shields.io/nuget/v/prise?style=flat-square)](https://www.nuget.org/packages/Prise)
  [![Prise Nuget Version Beta](https://img.shields.io/nuget/vpre/prise?style=flat-square)](https://www.nuget.org/packages/Prise)

  [![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/merken/Prise/LICENSE)

</div>

---

<p align="center"> Prise, A .NET (Core) Plugin Framework.
    <br/> 
</p>

## 📝 Table of Contents
- [Advanced Plugins](#advanced-plugins)
    - [🔌 Plugin Services](#-plugin-services)
    - [🎩 Host Services](#-host-services)
    - [❕ Tips](#-tips)
  - [✅ Prerequisites](#-prerequisites)
  - [🚀 Creating Prise Plugin Packages](#-creating-prise-plugin-packages)
  - [🤔 How does Prise work?](#-how-does-prise-work)
    - [☑️ High-level run-through](#️-high-level-run-through)
  - [📜 Examples](#-examples)
  - [✍️ Authors](#️-authors)

## 🧐 About

**Prise** is a plugin framework for .NET (Core) applications, written in .NET (Core). The goal of Prise, is enable you to write **decoupled pieces of code** using the least amount of effort, whilst **maximizing the customizability and backwards-compatability**. Prise helps you load plugins from foreign assemblies. It is built to decouple the local and remote dependencies, and strives to avoid assembly mismatches.

Main features:

- Easy setup
- Fully customizable loading of plugins
- Loading plugins from Prise Plugin Packages (Nuget) (TODO What is a Prise Plugin Package)
- Supporting backwards compatibility for older (previously written) plugins

## 🏁 Getting Started

A plugin system, of any kind, consists of the following components:
- 🎩 **Host**: The Host application, your Console app, ASP.NET Core Web app, an Azure Function Host, ...
- 📝 **Contract**: The shared library between the Host and the Plugin, contains all operations the Host can invoke on the Plugin
- 🔌 **Plugin**: A plugin assembly that contains at least 1 implementation of the Contract

### 🌤️ The Weather Project

[ASP.NET Core scaffolds](https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-3.1&tabs=visual-studio) a ```WeatherForecastController``` when a new ```webapi``` project is created, our example TODO LINK will extend this project with an [OpenWeatherMap](https://openweathermap.org) Plugin.

Let's create the ```Weather.Api``` **Host** for our Weather project from scratch using the Weather webapi template from ASP.NET Core:

```
mkdir Weather
cd Weather
mkdir Weather.Api
cd Weather.Api
dotnet new webapi
```

<img src="createweatherapi.gif" alt="Create Weather Api Project"></a>

Next, we need to create the **Contract** ```Weather.Contract``` project:
```
cd ..
mkdir Weather.Contract
cd Weather.Contract
dotnet new classlib -f netstandard2.0
```

<img src="createweathercontract.gif" alt="Create Weather Contract Project"></a>

This project does not need to be anything other than a netstandard library, it does not require any dependencies either.

Now, we create our plugin ```OpenWeather.Plugin``` project:

```
cd ..
mkdir OpenWeather.Plugin
cd OpenWeather.Plugin
dotnet new classlib -f netcoreapp3.1
```

<img src="createopenweatherplugin.gif" alt="Create OpenWeather Plugin Project"></a>

While you **can** create ```netstandard``` plugins, it is best to choose a specific framework for your **Plugin**, like ```netcoreapp2.1```, ```netcoreapp3.1``` or ```net5.0```.

When we open the ```Weather``` directory in VS Code, we can see that projects are ready:

<img src="weatherproject.png" alt="Create Weather Contract Project"></a>

**Prise** comes with a toolset for VS Code and Visual Studio, install the [Prise Publish Plugin Extension for VS Code](https://marketplace.visualstudio.com/items?itemName=MRKN.prise-publishpluginextension) from the Marketplace.

Now we're all set to start plugging in Prise!

### 📝 Defining the Contract

Create a new C# interface file inside the Weather.Contract project named ```IWeatherPlugin.cs```:
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weather.Contract
{
    public class WeatherForecast
    {
        /// <summary>
        /// Day of the week
        /// </summary>
        /// <value></value>
        public DateTime Date { get; set; }

        /// <summary>
        /// Overall temperature in Celsius
        /// </summary>
        /// <value></value>
        public int TemperatureC { get; set; }

        /// <summary>
        /// Short summary of the weather for that day
        /// </summary>
        /// <value></value>
        public string Summary { get; set; }
    }

    public interface IWeatherPlugin
    {
        Task<IEnumerable<WeatherForecast>> GetWeatherFor(string location);
    }
}
```
Each **Plugin** is responsible for returning a list of ```WeatherForecast``` for a given location. We will implement the OpenWeather.Plugin later.


### 🎩 Setting up the Weather.Api host
Add the Prise NuGet package to Weather.Api **🎩 Host** project:

```
dotnet add package Prise
```

Add the Weather.Contract project reference:

```
dotnet add reference ../Weather.Contract
```


Add the following lines to your Startup.cs file:
<pre>
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();
    <b>services.AddPrise();</b>
}
</pre>

You can now inject the [IPluginLoader](https://raw.githubusercontent.com/merken/Prise/v2/src/Prise/IPluginLoader.cs) into your ```WeatherForecastController```:
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
// Add Prise
using Prise;
// Add the Contract
using Weather.Contract;

namespace Weather.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        // Inject the Prise Default IPluginLoader
        private readonly IPluginLoader weatherPluginLoader;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IPluginLoader weatherPluginLoader)
        {
            _logger = logger;
            this.weatherPluginLoader = weatherPluginLoader;
        }

        // ... removed for brevity
```

The ```IPluginLoader``` is a Prise type that allows you find and load Plugins.

We will publish the ```OpenWeather.Plugin``` to the ```_dist``` folder created at the root of our project, which means that the ```WeatherForecastController``` needs to look for Plugins from that directory on the local disk.

Let's write some code inside the ```WeatherForecastController``` to do that:

Change the default implementation of the ```Get``` method.
```csharp
 // Add the location parameter to the route
[HttpGet("{location}")]
public async Task<IEnumerable<WeatherForecast>> Get(string location)
{
    // pathToBinDebug = Weather.Api/bin/Debug/netcoreapp3.1
    var pathToBinDebug = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    // pathToDist = _dist
    var pathToDist = Path.GetFullPath("../../../../_dist", pathToBinDebug);
    // scanResult should contain the information about the OpenWeather.Plugin
    var scanResult = await this.weatherPluginLoader.FindPlugin<IWeatherPlugin>(pathToDist);

    if (scanResult == null)
    {
        _logger.LogWarning($"No plugin was found for type {typeof(IWeatherPlugin).Name}");
        return null;
    }

    // Load the IWeatherPlugin
    var plugin = await this.weatherPluginLoader.LoadPlugin<IWeatherPlugin>(scanResult);

    // Invoke the IWeatherPlugin
    return await plugin.GetWeatherFor(location);
}
```

Remove the ~```WeatherForecast.cs```~ file located in the root of the ```Weather.Api``` project, this forces the ```WeatherForecastController``` to use the ```WeatherForecast``` class from the ```Weather.Contract```.


### 🔌 Your First Prise Plugin

Add the ```Prise.Plugin``` package to the ```OpenWeather.Plugin``` project:
```
dotnet add package Prise.Plugin
```

And add the reference to the ```Weather.Contract```:
```
dotnet add reference ../Weather.Contract
```

Change the filename from ```Class1.cs``` to ```OpenWeatherPlugin.cs``` and make the file to look like a Plugin:
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Prise.Plugin;
using Weather.Contract;

namespace OpenWeather.Plugin
{
    // This makes the Plugin discoverable for Prise
    [Plugin(PluginType = typeof(IWeatherPlugin))]
    public class OpenWeatherPlugin : IWeatherPlugin
    {
        // The Contract method we need to implement
        public async Task<IEnumerable<WeatherForecast>> GetWeatherFor(string location)
        {
            var openWeatherApi = "https://api.openweathermap.org/data/2.5/";
            var apiKey = "fd1f28913df6e270e48ea04536e3daba";
            var forecastEndpoint = $"forecast?q={location}&appid={apiKey}";

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(openWeatherApi);

            var response = await httpClient.GetAsync(forecastEndpoint);
            if (!response.IsSuccessStatusCode)
                throw new Exception("API did not respond with success.");

            var content = await response.Content.ReadAsStringAsync();
            var openWeatherModel = JsonSerializer.Deserialize<OpenWeatherResponseModel>(content);
            var results = openWeatherModel.list.Select(m => MapToWeatherForecast(m));
            var resultsPerDay = results.GroupBy(r => r.Date.DayOfWeek).Select(g => g.First());

            return resultsPerDay;
        }

        private WeatherForecast MapToWeatherForecast(OpenWeatherModel model)
        {
            return new WeatherForecast
            {
                Date = FromUnix(model.dt),
                Summary = model.weather.ElementAt(0).description,
                TemperatureC = FromKelvinToCelsius(model.main.temp)
            };
        }

        private DateTime FromUnix(long unixTimeStamp)
            => new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(unixTimeStamp).ToLocalTime();

        private int FromKelvinToCelsius(decimal kelvin)
            => (int)(kelvin - 273.15m);
    }

    class OpenWeatherForecast
    {
        public string main { get; set; }
        public string description { get; set; }
    }

    class OpenWeatherTemperature
    {
        // Temperature in Kelvin
        public decimal temp { get; set; }
    }

    class OpenWeatherModel
    {
        // Day of the week
        public long dt { get; set; }
        public OpenWeatherTemperature main { get; set; }
        public List<OpenWeatherForecast> weather { get; set; }
    }

    class OpenWeatherResponseModel
    {
        public List<OpenWeatherModel> list { get; set; }
    }
}

```

The implementation of the ```IWeatherPlugin``` interface is **optional**, as long as you expose a method called ```GetWeatherFor(string location)``` with the correct **return type** and **parameters**, Prise should be able to invoke this Plugin! 
**Prise** will look for a class that is annotated with the ```Plugin``` attribute that references the **Contract** (```IPlugin```).

This means that your Weather.Contract can grow and expand to support new logic, whilst remaining the ability to invoke older, previously built plugins.

### Wrapping it up

At this point we've determined the **Contract** (```Weather.Contract```), got the **Host** (```Weather.Api```) ready to look for ```IWeatherPlugin``` Plugins and invoke them, and we've written our first **Plugin** (```OpenWeather.Plugin```).
Now, we need to build the Plugin and copy it the the _dist folder so that the Host can find it.

With the [Prise Publish Plugin Extension for VS Code](https://marketplace.visualstudio.com/items?itemName=MRKN.prise-publishpluginextension) you can easily publish your Plugins from inside the editor.
**Right-click** on the ```OpenWeather.Plugin.csproj``` file inside of VS Code and select ```Create Prise Plugin File```
<img src="createpluginfile.gif" alt="Create Prise Plugin File"></a>
A new .json file will be scaffolded into the ```OpenWeather.Plugin``` directory named ```prise.plugin.json```.
This file contains **configuration** about how you want to **publish** your Plugin using the extension.

Open the ```prise.plugin.json``` file and change the ```publishDir``` setting to ```../_dist```. The ```publishDir``` setting can be a **relative** or an **absolute** path on the filesystem.
Each Plugin can have its own prise.plugin.json file.
<img src="editpluginfile.gif" alt="Edit Prise Plugin File"></a>

When this is configured, you should be able to Right-click the ```OpenWeather.Plugin.csproj``` again and select ```Publish Prise Plugin```.
<img src="publishplugin.gif" alt="Publish Prise Plugin File"></a>
This command **builds** and **publishes** your **Plugin** using the ```dotnet publish``` cli command, the output of the publish is copied over to the ```publishDir``` location.

Now we can ```dotnet run``` the ```Weather.Api``` project and send our first request!
<img src="dotnetrun.gif" alt="dotnet run"/>

Inside a browser navigate to https://localhost:5001/weatherforecast/brussels,be to see the forecast of Brussels.
<img src="brusselsweather.gif" alt="dotnet run"/>

```
cd Weather.Api
dotnet run
```


# Advanced Plugins

### 🔌 Plugin Services

Plugins can have their own services (dependencies), this helps keeping your Plugin organized and clean.

During activation, each Plugin gets a fresh ```IServiceCollection``` which it can use to register services to use when the Plugin is activated.

This collection can be configured using a ```PluginBootstrapper```.
<pre>
[Prise.Plugin.<b>PluginBootstrapper</b>(PluginType = typeof(<b>FooPlugin</b>))]
public class FooPluginBootstrapper : <b>Prise.Plugin.IPluginBootstrapper</b>
{
    public IServiceCollection <b>Bootstrap</b>(IServiceCollection services)
    {
        return services
            .AddTransient< HttpClient >(sp =>
            {
                var endpoint = "https://myapi.mydomain.com";
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(endpoint);
                return httpClient;
            });
    }
}
</pre>
In the example above we see a ```PluginBootstrapper``` for the ```FooPlugin``` called ```FooPluginBootstrapper```.
This bootstrapper is called before the ```FooPlugin``` is activated, Prise links these two objects via the ```PluginBootstrapperAttribute```.
Each plugin within an assembly (an assembly can contain multiple plugins) can have a bootstrapper linked to it.

Now we can edit the ````FooPlugin``` to have the ```HttpClient``` injected:
```
```


### 🎩 Host Services

In order to maximize backwards compatability, the **Plugin** should, ideally, not use any of the **Host** services.

In most cases, the **Plugin** needs to know about some configuration that is setup in the **Host** (```appsettings.json```). This is typically shared through a ```IConfigurationService``` from inside the **Contract**:
```
public interface IConfigurationService
{
    string GetConfigurationValueForKey(string key);
}
```

In order for the backwards compatability to work from the **Host** to the **Plugin**, **Prise** creates a [DispatchProxy](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.dispatchproxy?view=netcore-3.1) of the instantiated **Plugin**. All communication channels through this proxy. The [PriseProxy](https://github.com/merken/Prise/blob/v2/src/Prise.Proxy/PriseProxy.cs) is responsible for matching the methods invoked on the **Host** to the correct methods available on the **Plugin**.

When the **Plugin** needs to interact with the **Host**, the **exact reverse setup is required**, here we speak of a **ReverseProxy**, because it does the exact reverse of what the **Prise.Proxy** does.

To add support for **Host** Services in your Plugin, we use something called Field Injection, using Reflection:
<pre>
[Plugin(PluginType = typeof(IPlugin))]
public class FooPlugin : IPlugin
{
    // This field is injected using Prise Field Injection
    [<b>PluginService</b>(ServiceType = typeof(IConfigurationService), <b>ProxyType</b> = typeof(ConfigurationService))]
    private readonly <b>IConfigurationService</b> configurationService;

    private string dataFromConfig;
    
    [PluginActivated]
    public void ThisIsCalledWhenThePluginIsActivated()
    {
        // Get data from the IConfigurationService defined in the Host
        this.dataFromConfig = this.configurationService.GetConfigurationValueForKey("DataFromConfigFile");
    }

    public async Task<string> GetData()
    {
        await Task.Delay(1000); // Simulate network load
        return this.dataFromConfig;
    }
}
</pre>

You will need to create a **ReverseProxy** inside your **Plugin** in order to call the **Host** service:

<pre>
public class ConfigurationService : <b>ReverseProxy</b>, IConfigurationService
{
    public ConfigurationService(object hostService) : base(hostService) { }

    public string GetConfigurationValueForKey(string key)
    {
        <b>return this.InvokeOnHostService< string >(key);</b> // Return type and parameters must match
    }
}
</pre>

### ❕ Tips

- Keep your Plugins small
- Only use DTO's (Data Transfer Object) as part of the **Contract**
- **Avoid** using the following types in the **Contract**:
  - System.Linq.Expression
  - System.IO.Stream
  - System.EventHandler
  - Generics ```T Foo<T>(T foo)```
- All types within Prise are **IDisposable**, after invoking a Plugin, you should **dispose** these objects in order to support Hot-Swapping a Plugin

## ✅ Prerequisites

Prise supports the following frameworks (*):
- .NET Core 2.1
- .NET Core 3.1
- .NET 5.0

Prise runs on the following platforms (*):
- Windows
- macOS
- Linux

(*)
[2.1 support](https://github.com/dotnet/core/blob/master/release-notes/2.1/2.1-supported-os.md) | 
[3.1 support](https://github.com/dotnet/core/blob/master/release-notes/3.1/3.1-supported-os.md) | 
[5.0 support](https://github.com/dotnet/core/blob/master/release-notes/5.0/5.0-supported-os.md)

## 🚀 Creating Prise Plugin Packages
<a name="packages"></a>

Packages can be created easily using the toolset for Visual Studio or VS Code.
- [Visual Studio Prise Extension](https://marketplace.visualstudio.com/items?itemName=MRKN.PrisePublishPluginExtension)
- [VS Code Prise Extension](https://marketplace.visualstudio.com/items?itemName=MRKN.prise-publishpluginextension)

## 🤔 How does Prise work?
<a name="how"></a>

At the heart, Prise uses a **[DispatchProxy](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.dispatchproxy?view=netcore-3.1)** of your **contract**. Every call to a method channels through this proxy and is delegated to the loaded plugin.

Prise **does not require the plugin** to implement the interface provided by the contract, allowing you to extend the contract going forward, whilst still keeping your old plugins **backwards compatible**.

### ☑️ High-level run-through

Let's your Plugin Contract is the following:
```
public interface IPlugin
{
    Task<string> GetData();
}
```

- Prise scans a specified directory for types within assemblies that are annotated with the 

## 📜 Examples

- [🖥️ Console app using Prise](https://github.com/merken/Prise/tree/v2/samples/Example.Console)
- [⚡ Azure Function using Prise](https://github.com/merken/Prise/tree/v2/samples/Example.AzureFunction)
- [🌐 Web project using Prise](https://github.com/merken/Prise/tree/v2/samples/Example.Web)
- [🌐 WebApi project using Prise](https://github.com/merken/Prise/tree/v2/samples/Example.WebApi)
- [🌐 MVC using Prise](https://github.com/merken/Prise/tree/v2/samples/Example.Mvc.Controllers)
- [🌐 MVC Razor using Prise](https://github.com/merken/Prise/tree/v2/samples/Example.Mvc.Razor)

## ✍️ Authors

- [@merken](https://github.com/merken) - Idea & Initial work