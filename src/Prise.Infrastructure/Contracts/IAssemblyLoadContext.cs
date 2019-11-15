using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise.Infrastructure
{
    public interface IAssemblyLoadContext : IDisposable
    {
        Assembly LoadPluginAssembly(string pluginAssemblyName);
        Task<Assembly> LoadPluginAssemblyAsync(string pluginAssemblyName);
        void Unload();
    }
}
