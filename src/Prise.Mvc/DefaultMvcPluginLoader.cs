using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Prise.Core;
using Prise.Caching;
using Prise.AssemblyScanning;
using Prise.AssemblyLoading;
using System.Linq;
using Prise.Utils;

namespace Prise.Mvc
{
    public class DefaultMvcPluginLoader : IMvcPluginLoader
    {
        private readonly IAssemblyScanner assemblyScanner;
        private readonly IPluginTypeSelector pluginTypeSelector;
        private readonly IAssemblyLoader assemblyLoader;
        private readonly IPluginCache pluginCache;
        public DefaultMvcPluginLoader(IAssemblyScanner assemblyScanner,
                            IPluginTypeSelector pluginTypeSelector,
                            IAssemblyLoader assemblyLoader,
                            IPluginCache pluginCache)
        {
            this.assemblyScanner = assemblyScanner;
            this.pluginTypeSelector = pluginTypeSelector;
            this.assemblyLoader = assemblyLoader;
            this.pluginCache = pluginCache;
        }

        public async virtual Task<IEnumerable<AssemblyScanResult>> FindPlugins<T>(string pathToPlugins)
        {
            return (await this.assemblyScanner.Scan(new AssemblyScannerOptions
            {
                StartingPath = pathToPlugins,
                PluginType = typeof(T)
            }));
        }

        public async Task<IAssemblyShim> LoadPluginAssembly<T>(AssemblyScanResult plugin, Action<PluginLoadContext> configureLoadContext = null)
        {
            var pluginLoadContext = ToPluginLoadContext<T>(plugin);

            configureLoadContext?.Invoke(pluginLoadContext);

            pluginLoadContext.AddMvc();

            var pluginAssembly = await this.assemblyLoader.Load(pluginLoadContext);

            this.pluginCache.Add(pluginAssembly, pluginLoadContext.HostServices.Select(s => s.ServiceType));

            return pluginAssembly;
        }

        public async Task UnloadPluginAssembly<T>(AssemblyScanResult plugin)
        {
            var pluginLoadContext = ToPluginLoadContext<T>(plugin);
            await this.assemblyLoader.Unload(pluginLoadContext);

            var pathToAssembly = Path.Combine(plugin.AssemblyPath, plugin.AssemblyName);
            this.pluginCache.Remove(pathToAssembly);
        }

        private PluginLoadContext ToPluginLoadContext<T>(AssemblyScanResult plugin)
        {
            var hostFramework = HostFrameworkUtils.GetHostframeworkFromHost();
            var pathToAssembly = Path.Combine(plugin.AssemblyPath, plugin.AssemblyName);
            return PluginLoadContext.DefaultPluginLoadContext(pathToAssembly, typeof(T), hostFramework);
        }
    }
}