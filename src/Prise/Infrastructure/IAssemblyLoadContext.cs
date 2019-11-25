using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise.Infrastructure
{
    public interface IAssemblyLoadContext : IDisposable
    {
        Assembly LoadPluginAssembly(IPluginLoadContext pluginLoadContext);
        Task<Assembly> LoadPluginAssemblyAsync(IPluginLoadContext pluginLoadContext);
        void Unload();
    }
}
