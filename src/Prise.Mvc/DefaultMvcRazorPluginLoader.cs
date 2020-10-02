using System;
using System.Threading.Tasks;
using Prise.Core;
using Prise.Caching;
using Prise.AssemblyScanning;
using Prise.AssemblyLoading;
using System.Linq;

namespace Prise.Mvc
{
    public class DefaultMvcRazorPluginLoader : DefaultMvcPluginLoader
    {
        public DefaultMvcRazorPluginLoader(IAssemblyScanner assemblyScanner,
                                    IPluginTypeSelector pluginTypeSelector,
                                    IAssemblyLoader assemblyLoader,
                                    IPluginCache pluginCache) : base(assemblyScanner, pluginTypeSelector, assemblyLoader, pluginCache) { }

        public override async Task<IAssemblyShim> LoadPluginAssembly<T>(AssemblyScanResult plugin, Action<PluginLoadContext> configureLoadContext = null)
        {
            var pluginLoadContext = ToPluginLoadContext<T>(plugin);

            configureLoadContext?.Invoke(pluginLoadContext);

            pluginLoadContext
                .AddMvcRazorTypes();

            var pluginAssembly = await this.assemblyLoader.Load(pluginLoadContext);

            this.pluginCache.Add(pluginAssembly, pluginLoadContext.HostServices.Select(s => s.ServiceType));

            return pluginAssembly;
        }
    }
}