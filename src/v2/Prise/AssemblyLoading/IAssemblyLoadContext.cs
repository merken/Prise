using System;
using System.Threading.Tasks;
using Prise.Core;

namespace Prise.AssemblyLoading
{
    public interface IAssemblyLoadContext : IDisposable
    {
        Task<IAssemblyShim> LoadPluginAssembly(IPluginLoadContext loadContext, IAssemblyLoadStrategy pluginLoadStrategy = null, IPluginDependencyResolver pluginDependencyResolver = null);

        Task Unload();
    }
}