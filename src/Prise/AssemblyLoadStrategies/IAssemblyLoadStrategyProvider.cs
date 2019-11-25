using Prise.Infrastructure;

namespace Prise
{
    public interface IAssemblyLoadStrategyProvider
    {
        IAssemblyLoadStrategy ProvideAssemblyLoadStrategy(
            IPluginLoadContext pluginLoadContext,
            IPluginDependencyContext pluginDependencyContext);
    }

    public class DefaultAssemblyLoadStrategyProvider : IAssemblyLoadStrategyProvider
    {
        public IAssemblyLoadStrategy ProvideAssemblyLoadStrategy(
            IPluginLoadContext pluginLoadContext,
            IPluginDependencyContext pluginDependencyContext)
        {
            return new DefaultAssemblyLoadStrategy(pluginLoadContext, pluginDependencyContext);
        }
    }
}
