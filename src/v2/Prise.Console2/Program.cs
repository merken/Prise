using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Prise.AssemblyLoading;
using Prise.Console.Contract;
using Prise.Core;
using Prise.Utils;
using Prise.Web;
namespace Prise.Console2
{
    class Program
    {
        static async Task Main(string[] args)
        {

            var pathToThisProgram = Assembly.GetExecutingAssembly() // this assembly location (/bin/Debug/netcoreapp3.1)
                                                  .Location;
            var pathToExecutingDir = Path.GetDirectoryName(pathToThisProgram);
            var pathToPlugin = Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../Packages/dist/Prise.Plugin.Sql.Legacy"));

            var pluginAssembly = Path.Combine(pathToPlugin, "Prise.Plugin.Sql.Legacy.dll");
            var pluginContext = new PluginLoadContext(pluginAssembly, typeof(IStoragePlugin), HostFrameworkUtils.GetHostframeworkFromHost());
            var depContext = DefaultPluginDependencyContext.FromPluginLoadContext(pluginContext).Result;

            System.Console.WriteLine($"PLUGIN {depContext.FullPathToPluginAssembly}");
            System.Console.WriteLine($"HostDependencies");
            foreach (var p in depContext.HostDependencies)
                System.Console.WriteLine($"{p.DependencyName.Name}");

            System.Console.WriteLine($"");
            System.Console.WriteLine($"RemoteDependencies");
            foreach (var p in depContext.RemoteDependencies)
                System.Console.WriteLine($"{p.DependencyName.Name}");

            System.Console.WriteLine($"");
            System.Console.WriteLine($"PlatformDependencies");
            foreach (var p in depContext.PlatformDependencies)
                System.Console.WriteLine($"{p.DependencyPath} {p.DependencyNameWithoutExtension}");

            System.Console.WriteLine($"");
            System.Console.WriteLine($"PluginDependencies");
            foreach (var p in depContext.PluginDependencies)
                System.Console.WriteLine($"{p.DependencyPath} {p.DependencyNameWithoutExtension}");


            System.Console.WriteLine($"");
            System.Console.WriteLine($"PluginResourceDependencies");
            foreach (var p in depContext.PluginResourceDependencies)
                System.Console.WriteLine($"{p.Path}");

            var hostServices = new ServiceCollection();

            hostServices.AddTransient(typeof(IConfigurationService), typeof(AppSettingsConfigurationService));

            foreach (var hostService in hostServices)
                pluginContext
                    // A host type will always live inside the host
                    .AddHostTypes(new[] { hostService.ServiceType })
                    // The implementation type will always exist on the Host, since it will be created here
                    .AddHostTypes(new[] { hostService.ImplementationType ?? hostService.ImplementationInstance?.GetType() ?? hostService.ImplementationFactory?.Method.ReturnType });
            ;

            var priseLoader = new Prise.AssemblyLoading.DefaultAssemblyLoader();
            var activator = new Prise.Activation.DefaultPluginActivator();
            var assembly = priseLoader.Load(pluginContext).Result;
            var type = assembly.Assembly.GetType("Prise.Plugin.Sql.Legacy.SqlStoragePlugin");

            var options = new Activation.DefaultPluginActivationOptions
            {
                PluginType = type,
                PluginAssembly = assembly,
                ParameterConverter = new Prise.Infrastructure.JsonSerializerParameterConverter(),
                ResultConverter = new Prise.Infrastructure.JsonSerializerResultConverter(),
                HostServices = hostServices
            };

            var plugin = activator.ActivatePlugin<IStoragePlugin>(options).Result;

            System.Console.WriteLine(plugin.GetData(new Console.Contract.PluginObject
            {
                Text = "999"
            }).Result.Text);

        }
    }
}
