using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Prise.Utils;
using Prise.AssemblyScanning;
using Prise.Core;
using Prise.AssemblyLoading;

namespace Prise.Mvc
{
    public interface IMvcPluginLoader
    {
        Task<IEnumerable<AssemblyScanResult>> FindPlugins<T>(string pathToPlugins);
        Task<IAssemblyShim> LoadPluginAssembly<T>(AssemblyScanResult plugin, Action<PluginLoadContext> configureLoadContext = null);
        Task UnloadPluginAssembly<T>(AssemblyScanResult plugin);
    }
}