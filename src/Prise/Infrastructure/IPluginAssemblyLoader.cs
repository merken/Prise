using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise.Infrastructure
{
    public interface IPluginAssemblyLoader<T> : IDisposable
    {
        Assembly Load(IPluginLoadContext pluginLoadContext);
        Task<Assembly> LoadAsync(IPluginLoadContext pluginLoadContext);
        void Unload();
        Task UnloadAsync();
    }
}