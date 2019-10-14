using System.Reflection;
using System.Threading.Tasks;

namespace Prise.Infrastructure
{
    public interface IPluginAssemblyLoader<T>
    {
        Task<Assembly> Load(string pluginAssemblyName);
    }
}