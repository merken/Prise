using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Prise.Core;
using Prise.Proxy;
using Prise.Activation;
using Prise.AssemblyLoading;
using Prise.AssemblyScanning;
using Example.Contract;
using Prise.Utils;

namespace Example.AzureFunction
{
    public interface IPluginLoader
    {
        Task<AssemblyScanResult> FindPlugin<T>(string pathToPlugins, string plugin);
        Task<T> LoadPlugin<T>(AssemblyScanResult plugin, Action<PluginLoadContext> configureLoadContext = null);
    }

    public class FunctionPluginLoader : IPluginLoader
    {
        private readonly IConfigurationService configurationService;
        private readonly IAssemblyScanner assemblyScanner;
        private readonly IPluginTypeSelector pluginTypeSelector;
        private readonly IAssemblyLoader assemblyLoader;
        private readonly IParameterConverter parameterConverter;
        private readonly IResultConverter resultConverter;
        private readonly IPluginActivator pluginActivator;
        public FunctionPluginLoader(IConfigurationService configurationService,
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

        public async Task<AssemblyScanResult> FindPlugin<T>(string pathToPlugins, string plugin)
        {
            return (await this.assemblyScanner.Scan(new AssemblyScannerOptions
            {
                StartingPath = pathToPlugins,
                PluginType = typeof(T)
            })).FirstOrDefault(p => p.AssemblyPath.Split(Path.DirectorySeparatorChar).Last().Equals(plugin));
        }

        public async Task<T> LoadPlugin<T>(AssemblyScanResult plugin, Action<PluginLoadContext> configureLoadContext = null)
        {
            var hostFramework = HostFrameworkUtils.GetHostframeworkFromHost();
            var servicesForPlugin = new ServiceCollection();

            var pathToAssembly = Path.Combine(plugin.AssemblyPath, plugin.AssemblyName);
            var pluginLoadContext = PluginLoadContext.DefaultPluginLoadContext(pathToAssembly, typeof(T), hostFramework);
            // This allows the loading of netstandard plugins
            pluginLoadContext.IgnorePlatformInconsistencies = true;

            configureLoadContext?.Invoke(pluginLoadContext);

            var pluginAssembly = await this.assemblyLoader.Load(pluginLoadContext);
            var pluginTypes = this.pluginTypeSelector.SelectPluginTypes<T>(pluginAssembly);
            var firstPlugin = pluginTypes.FirstOrDefault();

            return await this.pluginActivator.ActivatePlugin<T>(new DefaultPluginActivationOptions
            {
                PluginType = firstPlugin,
                PluginAssembly = pluginAssembly,
                ParameterConverter = this.parameterConverter,
                ResultConverter = this.resultConverter,
                HostServices = pluginLoadContext.HostServices
            });
        }
    }
}