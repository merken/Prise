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

namespace Prise.Mvc
{
    public interface IMvcPluginLoader
    {
        Task<IEnumerable<AssemblyScanResult>> FindPlugins<T>(string pathToPlugins);
        IAsyncEnumerable<IAssemblyShim> LoadPluginAssembly(AssemblyScanResult plugin, Action<IServiceCollection, DefaultPluginLoadContext> configureLoadContext = null);
        IAsyncEnumerable<IAssemblyShim> UnloadPluginAssembly(AssemblyScanResult plugin);
    }

    public class DefaultMvcPluginLoader : IMvcPluginLoader
    {
        private readonly IAssemblyScanner assemblyScanner;
        private readonly IPluginTypeSelector pluginTypeSelector;
        private readonly IAssemblyLoader assemblyLoader;
        private readonly IParameterConverter parameterConverter;
        private readonly IResultConverter resultConverter;
        private readonly IPluginActivator pluginActivator;
        public DefaultMvcPluginLoader(IAssemblyScanner assemblyScanner,
                            IPluginTypeSelector pluginTypeSelector,
                            IAssemblyLoader assemblyLoader,
                            IParameterConverter parameterConverter,
                            IResultConverter resultConverter,
                            IPluginActivator pluginActivator)
        {
            this.assemblyScanner = assemblyScanner;
            this.pluginTypeSelector = pluginTypeSelector;
            this.assemblyLoader = assemblyLoader;
            this.parameterConverter = parameterConverter;
            this.resultConverter = resultConverter;
            this.pluginActivator = pluginActivator;
        }

        public async virtual Task<IEnumerable<AssemblyScanResult>> FindPlugins<T>(string pathToPlugins)
        {
            return (await this.assemblyScanner.Scan(new AssemblyScannerOptions
            {
                StartingPath = pathToPlugins,
                PluginType = typeof(T)
            }));
        }
        public async IAsyncEnumerable<IAssemblyShim> LoadPluginAssembly(AssemblyScanResult plugin, Action<IServiceCollection, DefaultPluginLoadContext> configureLoadContext = null)
        {
            var hostFramework = HostFrameworkUtils.GetHostframeworkFromHost();
            var servicesForPlugin = new ServiceCollection();

            var pathToAssembly = Path.Combine(plugin.AssemblyPath, plugin.AssemblyName);
            var pluginLoadContext = PluginLoadContext.DefaultPluginLoadContext(pathToAssembly, typeof(T), hostFramework);
            configureLoadContext?.Invoke(pluginLoadContext);

        }

    }
}