using System;
using System.Threading.Tasks;
using Prise.Core;

namespace Prise.AssemblyLoading
{
    public interface IAssemblyLoadContext : IDisposable
    {
        /// <summary>
        /// Loads a specific plugin assembly into the current IAssemblyLoader
        /// </summary>
        /// <param name="loadContext">The loadcontext for the plugin</param>
        /// <returns></returns>
        Task<IAssemblyShim> LoadPluginAssembly(IPluginLoadContext loadContext);

        /// <summary>
        /// Unloads all assemblies that were loaded for this plugin
        /// </summary>
        /// <returns></returns>
        Task Unload();
    }
}