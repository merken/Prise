using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Prise;
using Prise.Core;
using Prise.AssemblyScanning;
using Prise.Activation;
using Prise.AssemblyLoading;
using Prise.DependencyInjection;
using Example.Contract;
using Microsoft.Extensions.Configuration;
using Prise.Utils;

namespace Example.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var mainServiceCollection = new ServiceCollection()
                                .AddPrise()
                                .AddSingleton<IConfiguration>(new ConfigurationBuilder()
                                                                .AddJsonFile("appsettings.json")
                                                                .Build())
                                .AddScoped<IConfigurationService, AppSettingsConfigurationService>();

            var serviceProvider = mainServiceCollection.BuildServiceProvider();

            var pathToDist = GetPathToDist();
            var hostFramework = HostFrameworkUtils.GetHostframeworkFromType(typeof(Program));

            var scanner = serviceProvider.GetRequiredService<IAssemblyScanner>();
            var loader = serviceProvider.GetRequiredService<IAssemblyLoader>();
            var activator = serviceProvider.GetRequiredService<IPluginActivator>();
            var typeSelector = serviceProvider.GetRequiredService<IPluginTypeSelector>();
            var configurationService = serviceProvider.GetRequiredService<IConfigurationService>();

            var results = await scanner.Scan(new AssemblyScannerOptions
            {
                StartingPath = pathToDist,
                PluginType = typeof(IPlugin)
            });

            foreach (var result in results)
            {
                var pathToAssembly = Path.Combine(result.AssemblyPath, result.AssemblyName);

                var pluginLoadContext = PluginLoadContext.DefaultPluginLoadContext(pathToAssembly, typeof(IPlugin), hostFramework);
                // This allows the loading of netstandard plugins
                pluginLoadContext.IgnorePlatformInconsistencies = true;

                pluginLoadContext.AddHostServices(mainServiceCollection, new[] { typeof(IConfigurationService) });

                var pluginAssembly = await loader.Load(pluginLoadContext);

                var pluginTypes = typeSelector.SelectPluginTypes<IPlugin>(pluginAssembly);
                foreach (var pluginType in pluginTypes)
                {
                    try
                    {
                        var pluginInstance = await activator.ActivatePlugin<IPlugin>(new DefaultPluginActivationOptions
                        {
                            PluginType = pluginType,
                            PluginAssembly = pluginAssembly,
                            ParameterConverter = DefaultFactories.DefaultParameterConverter(),
                            ResultConverter = DefaultFactories.DefaultResultConverter(),
                            HostServices = pluginLoadContext.HostServices
                        });

                        var pluginResults = await pluginInstance.GetAll();

                        foreach (var pluginResult in pluginResults)
                            System.Console.WriteLine($"{pluginResult.Text}");
                    }
                    catch (PluginActivationException pex) { }
                }
            }
        }

        private static string GetPathToDist()
        {
            var pathToThisProgram = Assembly.GetExecutingAssembly() // this assembly location (/bin/Debug/netcoreapp3.1)
                                        .Location;
            var pathToExecutingDir = Path.GetDirectoryName(pathToThisProgram);
            return Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../Plugins/dist"));
        }
    }
}
