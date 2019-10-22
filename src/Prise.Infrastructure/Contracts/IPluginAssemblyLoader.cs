using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise.Infrastructure
{
    public interface IPluginAssemblyLoader<T> : IDisposable
    {
        Assembly Load(string pluginAssemblyName);
        Task<Assembly> LoadAsync(string pluginAssemblyName);
        void Unload();
        Task UnloadAsync();
    }
}