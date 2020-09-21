using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Prise.Activation;
using Prise.AssemblyLoading;
using Prise.AssemblyScanning;
using Prise.Console.Contract;
using Prise.Utils;

namespace Prise.Functions.Example
{
    public interface IPluginLoader
    {
        Task<Core.AssemblyScanResult> FindPlugin<T>(string pathToPlugins, string plugin);
        Task<T> LoadPlugin<T>(Core.AssemblyScanResult plugin);
    }

    public class FunctionPluginLoader : IPluginLoader
    {
        private readonly IConfigurationService configurationService;
        private readonly IAssemblyScanner assemblyScanner;
        private readonly IAssemblyLoader assemblyLoader;
        private readonly IPluginActivator pluginActivator;
        public FunctionPluginLoader(IConfigurationService configurationService,
                               IAssemblyScanner assemblyScanner,
                               IAssemblyLoader assemblyLoader,
                               IPluginActivator pluginActivator)
        {
            this.configurationService = configurationService;
            this.assemblyScanner = assemblyScanner;
            this.assemblyLoader = assemblyLoader;
            this.pluginActivator = pluginActivator;
        }

        public async Task<Core.AssemblyScanResult> FindPlugin<T>(string pathToPlugins, string plugin)
        {
            return (await this.assemblyScanner.Scan(new AssemblyScannerOptions
            {
                StartingPath = pathToPlugins,
                PluginType = typeof(T)
            })).FirstOrDefault(p => p.AssemblyPath.Split(Path.DirectorySeparatorChar).Last().Equals(plugin));
        }

        public async Task<T> LoadPlugin<T>(Core.AssemblyScanResult plugin)
        {
            var hostFramework = HostFrameworkUtils.GetHostframeworkFromHost();
            var servicesForPlugin = new ServiceCollection();

            var pathToAssembly = Path.Combine(plugin.AssemblyPath, plugin.AssemblyName);

            var pluginLoadContext = Prise.Core.PluginLoadContext.DefaultPluginLoadContext(pathToAssembly, typeof(T), hostFramework);
            // This allows the loading of netstandard plugins
            pluginLoadContext.IgnorePlatformInconsistencies = true;
            pluginLoadContext.AddHostService(servicesForPlugin, typeof(IConfigurationService), this.configurationService.GetType());

            var pluginAssembly = await this.assemblyLoader.Load(pluginLoadContext);
            var pluginTypeSelector = new Prise.Core.DefaultPluginTypeSelector();

            var pluginTypes = pluginTypeSelector.SelectPluginTypes<T>(pluginAssembly);
            var firstPlugin = pluginTypes.FirstOrDefault();

            return await this.pluginActivator.ActivatePlugin<T>(new Activation.DefaultPluginActivationOptions
            {
                PluginType = firstPlugin,
                PluginAssembly = pluginAssembly,
                ParameterConverter = new Prise.Infrastructure.JsonSerializerParameterConverter(),
                ResultConverter = new Prise.Infrastructure.JsonSerializerResultConverter(),
                HostServices = servicesForPlugin
            });
        }
    }
}