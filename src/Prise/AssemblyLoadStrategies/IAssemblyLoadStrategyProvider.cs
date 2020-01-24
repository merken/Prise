using Prise.Infrastructure;

namespace Prise
{
    public interface IAssemblyLoadStrategyProvider
    {
        IAssemblyLoadStrategy ProvideAssemblyLoadStrategy(
            IPluginLogger logger,
            IPluginLoadContext pluginLoadContext,
            IPluginDependencyContext pluginDependencyContext);
    }

    public class DefaultAssemblyLoadStrategyProvider : IAssemblyLoadStrategyProvider
    {
        public IAssemblyLoadStrategy ProvideAssemblyLoadStrategy(
            IPluginLogger logger,
            IPluginLoadContext pluginLoadContext,
            IPluginDependencyContext pluginDependencyContext)
        {
            return new DefaultAssemblyLoadStrategy(logger, pluginLoadContext, pluginDependencyContext);
        }
    }
}
