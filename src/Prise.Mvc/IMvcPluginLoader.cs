using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Prise.Core;

namespace Prise.Mvc
{
    public interface IMvcPluginLoader
    {
        Task<IEnumerable<AssemblyScanResult>> FindPlugins<T>(string pathToPlugins);
        Task<IAssemblyShim> LoadPluginAssembly<T>(AssemblyScanResult plugin, Action<PluginLoadContext> configureLoadContext = null);
        Task UnloadPluginAssembly<T>(AssemblyScanResult plugin);
    }
}