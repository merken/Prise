using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Prise.Activation;
using Prise.AssemblyLoading;
using Prise.AssemblyScanning;
using Prise.Console.Contract;
using Prise.Utils;
using Prise.DependencyInjection;

namespace Prise.Web.Services
{
    public interface IPluginLoader
    {
        Task<IEnumerable<Core.AssemblyScanResult>> FindPlugins<T>(string pathToPlugins);
        Task<T> LoadPlugin<T>(Core.AssemblyScanResult plugin);
    }

    public class PluginLoader : IPluginLoader
    {
        private readonly IConfigurationService configurationService;
        private readonly IAssemblyScanner assemblyScanner;
        private readonly IAssemblyLoader assemblyLoader;
        private readonly IPluginActivator pluginActivator;
        public PluginLoader(IConfigurationService configurationService,
                               IAssemblyScanner assemblyScanner,
                               IAssemblyLoader assemblyLoader,
                               IPluginActivator pluginActivator)
        {
            this.configurationService = configurationService;
            this.assemblyScanner = assemblyScanner;
            this.assemblyLoader = assemblyLoader;
            this.pluginActivator = pluginActivator;
        }

        public async Task<IEnumerable<Core.AssemblyScanResult>> FindPlugins<T>(string pathToPlugins)
        {
            return (await this.assemblyScanner.Scan(new AssemblyScannerOptions
            {
                StartingPath = pathToPlugins,
                PluginType = typeof(T)
            }));
        }

        public async Task<T> LoadPlugin<T>(Core.AssemblyScanResult plugin)
        {
            var hostFramework = HostFrameworkUtils.GetHostframeworkFromHost();
            var servicesForPlugin = new ServiceCollection();

            var pathToAssembly = Path.Combine(plugin.AssemblyPath, plugin.AssemblyName);

            var pluginLoadContext = Prise.Core.PluginLoadContext.DefaultPluginLoadContext(pathToAssembly, typeof(T), hostFramework);
            // This allows the loading of netstandard plugins
            pluginLoadContext.IgnorePlatformInconsistencies = true;
            // Adds the Host service (IConfigurationService) as a singleton
            pluginLoadContext.AddHostService<IConfigurationService>(servicesForPlugin, this.configurationService);

            var pluginAssembly = await this.assemblyLoader.Load(pluginLoadContext);
            var pluginTypeSelector = new Prise.Core.DefaultPluginTypeSelector();

            var pluginTypes = pluginTypeSelector.SelectPluginTypes<T>(pluginAssembly);
            var firstPlugin = pluginTypes.FirstOrDefault();

            return await this.pluginActivator.ActivatePlugin<T>(new Activation.DefaultPluginActivationOptions
            {
                PluginType = firstPlugin,
                PluginAssembly = pluginAssembly,
                ParameterConverter = DefaultFactories.DefaultParameterConverter(),
                ResultConverter = DefaultFactories.DefaultResultConverter(),
                HostServices = servicesForPlugin
            });
        }
    }
}