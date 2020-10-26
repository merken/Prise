using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Prise;

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

            var loader = serviceProvider.GetRequiredService<IPluginLoader>();
            var configurationService = serviceProvider.GetRequiredService<IConfigurationService>();

            var results = await loader.FindPlugins<IPlugin>(pathToDist);

            foreach (var result in results)
            {
                try
                {
                    var plugin = await loader.LoadPlugin<IPlugin>(result, configure: (context) =>
                    {
                        context.IgnorePlatformInconsistencies = true;
                        context.AddHostServices(mainServiceCollection, new[] { typeof(IConfigurationService) });
                    });

                    var pluginResults = await plugin.GetAll();

                    foreach (var pluginResult in pluginResults)
                        System.Console.WriteLine($"{pluginResult.Text}");
                }
                catch (PluginActivationException pex) { }
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
