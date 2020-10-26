using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

using Prise.Caching;
using Prise.AssemblyScanning;
using Prise.AssemblyLoading;
using System.Linq;
using Prise.Utils;

namespace Prise.Mvc
{
    public class DefaultMvcPluginLoader : IMvcPluginLoader
    {
        protected readonly IAssemblyScanner assemblyScanner;
        protected readonly IPluginTypeSelector pluginTypeSelector;
        protected readonly IAssemblyLoader assemblyLoader;
        protected readonly IPluginCache pluginCache;
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

        public async virtual Task<IAssemblyShim> LoadPluginAssembly<T>(AssemblyScanResult plugin, Action<PluginLoadContext> configureLoadContext = null)
        {
            var pluginLoadContext = ToPluginLoadContext<T>(plugin);

            configureLoadContext?.Invoke(pluginLoadContext);

            pluginLoadContext.AddMvcTypes();

            var pluginAssembly = await this.assemblyLoader.Load(pluginLoadContext);

            this.pluginCache.Add(pluginAssembly, pluginLoadContext.HostServices.Select(s => s.ServiceType));

            return pluginAssembly;
        }

        public async virtual Task UnloadPluginAssembly<T>(AssemblyScanResult plugin)
        {
            var pluginLoadContext = ToPluginLoadContext<T>(plugin);
            await this.assemblyLoader.Unload(pluginLoadContext);

            var pathToAssembly = Path.Combine(plugin.AssemblyPath, plugin.AssemblyName);
            this.pluginCache.Remove(pathToAssembly);
        }

        protected PluginLoadContext ToPluginLoadContext<T>(AssemblyScanResult plugin)
        {
            var hostFramework = HostFrameworkUtils.GetHostframeworkFromHost();
            var pathToAssembly = Path.Combine(plugin.AssemblyPath, plugin.AssemblyName);
            return PluginLoadContext.DefaultPluginLoadContext(pathToAssembly, typeof(T), hostFramework);
        }
    }
}