using System;
using System.Threading.Tasks;

namespace Prise.AssemblyLoading
{
    public interface IPluginAssemblyLoader : IDisposable
    {
        Task<IAssemblyShim> Load(IPluginLoadContext loadContext);
        Task Unload(IPluginLoadContext loadContext);
    }
}