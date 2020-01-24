using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise.Infrastructure
{
    public interface IPluginAssemblyLoader<T> : IDisposable
    {
        /// <summary>
        /// Loads the requested plugin load context in a separate AssemblyLoadContext.
        /// This context can be unloaded afterwards by specifying the name of the Plugin Assembly
        /// </summary>
        /// <param name="pluginLoadContext"></param>
        /// <returns>The plugin assembly loaded up from a new AssemblyLoadContext</returns>
        Assembly Load(IPluginLoadContext pluginLoadContext);
        Task<Assembly> LoadAsync(IPluginLoadContext pluginLoadContext);
        
        /// <summary>
        /// Unloads a specific plugin assembly that was previously loaded
        /// </summary>
        /// <param name="pluginAssemblyName"></param>
        void Unload(string pluginAssemblyName);
        Task UnloadAsync(string pluginAssemblyName);

        /// <summary>
        /// Unloads all AssemblyLoadContext's associated to this instance (can be multiple if scope == Singleton)
        /// </summary>
        void UnloadAll();
        Task UnloadAllAsync();
    }
}