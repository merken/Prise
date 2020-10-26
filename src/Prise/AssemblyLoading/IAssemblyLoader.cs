using System;
using System.Threading.Tasks;


namespace Prise.AssemblyLoading
{
    public interface IAssemblyLoader : IDisposable
    {
        Task<IAssemblyShim> Load(IPluginLoadContext pluginLoadContext);
        Task Unload(IPluginLoadContext pluginLoadContext);
    }
}