using System;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace Prise.Mvc
{
    public interface IMvcPluginLoader
    {
        Task<IEnumerable<AssemblyScanResult>> FindPlugins<T>(string pathToPlugins);
        Task<IAssemblyShim> LoadPluginAssembly<T>(AssemblyScanResult plugin, Action<PluginLoadContext> configure = null);
        Task UnloadPluginAssembly<T>(AssemblyScanResult plugin);
    }
}