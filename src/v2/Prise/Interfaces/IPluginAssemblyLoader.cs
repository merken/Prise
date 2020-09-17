using System;
using System.Threading.Tasks;

namespace Prise.V2
{
    public interface IPluginAssemblyLoader : IDisposable
    {
        Task<IAssemblyShim> Load(IPluginLoadContext loadContext);
        Task Unload(IPluginLoadContext loadContext);
    }
}