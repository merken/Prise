using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Prise.Utils;
using Prise.Example.Contract;
using Prise.AssemblyScanning;
using Prise.Core;
using Prise.AssemblyLoading;
using Prise.Proxy;
using Prise.Activation;
using Microsoft.Extensions.DependencyInjection;

namespace Prise.Example.Razor.Services
{
    public interface IPluginLoader
    {
        Task<IEnumerable<AssemblyScanResult>> FindPlugins<T>(string pathToPlugins);
        IAsyncEnumerable<T> LoadPlugins<T>(AssemblyScanResult plugin);
    }

    public class PluginLoader : IPluginLoader
    {
        private readonly IConfigurationService configurationService;
        private readonly IAssemblyScanner assemblyScanner;
        private readonly IPluginTypeSelector pluginTypeSelector;
        private readonly IAssemblyLoader assemblyLoader;
        private readonly IParameterConverter parameterConverter;
        private readonly IResultConverter resultConverter;
        private readonly IPluginActivator pluginActivator;
        public PluginLoader(IConfigurationService configurationService,
                            IAssemblyScanner assemblyScanner,
                            IPluginTypeSelector pluginTypeSelector,
                            IAssemblyLoader assemblyLoader,
                            IParameterConverter parameterConverter,
                            IResultConverter resultConverter,
                            IPluginActivator pluginActivator)
        {
            this.configurationService = configurationService;
            this.assemblyScanner = assemblyScanner;
            this.pluginTypeSelector = pluginTypeSelector;
            this.assemblyLoader = assemblyLoader;
            this.parameterConverter = parameterConverter;
            this.resultConverter = resultConverter;
            this.pluginActivator = pluginActivator;
        }

        public async Task<IEnumerable<AssemblyScanResult>> FindPlugins<T>(string pathToPlugins)
        {
            return (await this.assemblyScanner.Scan(new AssemblyScannerOptions
            {
                StartingPath = pathToPlugins,
                PluginType = typeof(T)
            }));
        }

        public async IAsyncEnumerable<T> LoadPlugins<T>(AssemblyScanResult plugin)
        {
            var hostFramework = HostFrameworkUtils.GetHostframeworkFromHost();
            var servicesForPlugin = new ServiceCollection();

            var pathToAssembly = Path.Combine(plugin.AssemblyPath, plugin.AssemblyName);
            var pluginLoadContext = PluginLoadContext.DefaultPluginLoadContext(pathToAssembly, typeof(T), hostFramework);
            // This allows the loading of netstandard plugins
            pluginLoadContext.IgnorePlatformInconsistencies = true;
            
            // Add this private field to collection
            pluginLoadContext.AddHostService<IConfigurationService>(servicesForPlugin, this.configurationService);

            var pluginAssembly = await this.assemblyLoader.Load(pluginLoadContext);
            var pluginTypes = this.pluginTypeSelector.SelectPluginTypes<T>(pluginAssembly);

            foreach (var pluginType in pluginTypes)
                yield return await this.pluginActivator.ActivatePlugin<T>(new DefaultPluginActivationOptions
                {
                    PluginType = pluginType,
                    PluginAssembly = pluginAssembly,
                    ParameterConverter = this.parameterConverter,
                    ResultConverter = this.resultConverter,
                    HostServices = servicesForPlugin
                });
        }
    }
}