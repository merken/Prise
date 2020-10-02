using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Prise.Utils;
using Example.Contract;
using Prise.AssemblyScanning;
using Prise.Core;
using Prise.AssemblyLoading;
using Prise.Proxy;
using Prise.Activation;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Example.WebApi
{
    public interface IPluginLoader
    {
        Task<IEnumerable<AssemblyScanResult>> FindPlugins<T>(string pathToPlugins);
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
        public PluginLoader(IAssemblyScanner assemblyScanner,
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

        public async Task<IEnumerable<AssemblyScanResult>> FindPlugins<T>(string pathToPlugins)
        {
            return (await this.assemblyScanner.Scan(new AssemblyScannerOptions
            {
                StartingPath = pathToPlugins,
                PluginType = typeof(T)
            }));
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