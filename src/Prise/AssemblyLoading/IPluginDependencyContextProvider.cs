using System.Threading.Tasks;

namespace Prise.AssemblyLoading
{
    public interface IPluginDependencyContextProvider
    {
        Task<IPluginDependencyContext> FromPluginLoadContext(IPluginLoadContext pluginLoadContext);
    }
}