using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Prise.Utils;
using Prise.AssemblyScanning;
using Prise.Core;
using Prise.AssemblyLoading;
using Prise.Proxy;
using Prise.Activation;
using System;
using System.Linq;

namespace Prise.IntegrationTestsHost.PluginLoaders
{
    public interface IPluginLoader
    {
        Task<IEnumerable<AssemblyScanResult>> FindPlugins<T>(string pathToPlugins);
        Task<T> LoadPlugin<T>(AssemblyScanResult plugin, Action<PluginLoadContext> configureLoadContext = null);
        IAsyncEnumerable<T> LoadPlugins<T>(AssemblyScanResult plugin, Action<PluginLoadContext> configureLoadContext = null);
    }

    public class PluginLoader : IPluginLoader
    {
        private readonly IAssemblyScanner assemblyScanner;
        private readonly IPluginTypeSelector pluginTypeSelector;
        private readonly IAssemblyLoader assemblyLoader;
        private readonly IParameterConverter parameterConverter;
        private readonly IResultConverter resultConverter;
        private readonly IPluginActivator pluginActivator;
        private readonly IHostFrameworkProvider hostFrameworkProvider;
        private readonly string pluginsDirectory;

        public PluginLoader(IAssemblyScanner assemblyScanner,
                            IPluginTypeSelector pluginTypeSelector,
                            IAssemblyLoader assemblyLoader,
                            IParameterConverter parameterConverter,
                            IResultConverter resultConverter,
                            IPluginActivator pluginActivator,
                            IHostFrameworkProvider hostFrameworkProvider)
        {
            this.assemblyScanner = assemblyScanner;
            this.pluginTypeSelector = pluginTypeSelector;
            this.assemblyLoader = assemblyLoader;
            this.parameterConverter = parameterConverter;
            this.resultConverter = resultConverter;
            this.pluginActivator = pluginActivator;
            this.hostFrameworkProvider = hostFrameworkProvider;
            this.pluginsDirectory = Path.GetFullPath("../../../../dist", AppDomain.CurrentDomain.BaseDirectory);
        }

        public async Task<IEnumerable<AssemblyScanResult>> FindPlugins<T>(string pathToPlugin)
        {
            return (await this.assemblyScanner.Scan(new AssemblyScannerOptions
            {
                StartingPath = Path.Combine(this.pluginsDirectory, pathToPlugin),
                PluginType = typeof(T)
            }));
        }

        public async Task<T> LoadPlugin<T>(AssemblyScanResult plugin, Action<PluginLoadContext> configurePluginLoadContext = null)
        {
            var hostFramework = this.hostFrameworkProvider.ProvideHostFramework();
            var pathToAssembly = Path.Combine(plugin.AssemblyPath, plugin.AssemblyName);
            var pluginLoadContext = PluginLoadContext.DefaultPluginLoadContext(pathToAssembly, typeof(T), hostFramework);
            // This allows the loading of netstandard plugins
            pluginLoadContext.IgnorePlatformInconsistencies = true;

            configurePluginLoadContext?.Invoke(pluginLoadContext);

            var pluginAssembly = await this.assemblyLoader.Load(pluginLoadContext);
            var pluginTypes = this.pluginTypeSelector.SelectPluginTypes<T>(pluginAssembly);
            var pluginType = pluginTypes.FirstOrDefault(p => p.Name == plugin.PluginType.Name);
            if (pluginType == null)
                throw new ArgumentNullException($"{plugin.PluginType.Name} Not found!");
            return await this.pluginActivator.ActivatePlugin<T>(new DefaultPluginActivationOptions
            {
                PluginType = pluginType,
                PluginAssembly = pluginAssembly,
                ParameterConverter = this.parameterConverter,
                ResultConverter = this.resultConverter,
                HostServices = pluginLoadContext.HostServices
            });
        }

        public async IAsyncEnumerable<T> LoadPlugins<T>(AssemblyScanResult plugin, Action<PluginLoadContext> configurePluginLoadContext = null)
        {
            var hostFramework = HostFrameworkUtils.GetHostframeworkFromHost();
            var pathToAssembly = Path.Combine(plugin.AssemblyPath, plugin.AssemblyName);
            var pluginLoadContext = PluginLoadContext.DefaultPluginLoadContext(pathToAssembly, typeof(T), hostFramework);
            // This allows the loading of netstandard plugins
            pluginLoadContext.IgnorePlatformInconsistencies = true;

            configurePluginLoadContext?.Invoke(pluginLoadContext);

            var pluginAssembly = await this.assemblyLoader.Load(pluginLoadContext);
            var pluginTypes = this.pluginTypeSelector.SelectPluginTypes<T>(pluginAssembly);

            foreach (var pluginType in pluginTypes)
                yield return await this.pluginActivator.ActivatePlugin<T>(new DefaultPluginActivationOptions
                {
                    PluginType = pluginType,
                    PluginAssembly = pluginAssembly,
                    ParameterConverter = this.parameterConverter,
                    ResultConverter = this.resultConverter,
                    HostServices = pluginLoadContext.HostServices
                });
        }
    }
}