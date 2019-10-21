using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise.Infrastructure
{
    public interface IPluginAssemblyLoader<T> : IDisposable
    {
        Task<Assembly> Load(string pluginAssemblyName);
        Task Unload();
    }
}