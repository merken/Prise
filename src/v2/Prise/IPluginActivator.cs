using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise
{
    public interface IPluginActivator
    {
        Task<T> ActivatePlugin<T>(ref Assembly pluginAssembly, Type pluginType = null);
    }
}