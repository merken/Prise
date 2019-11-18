using Prise.Infrastructure;

namespace Prise
{
    internal static class AssemblyLoadStrategyFactory
    {
        internal static IAssemblyLoadStrategy GetAssemblyLoadStrategy(
            DependencyLoadPreference dependencyLoadPreference,
            IPluginDependencyContext pluginDependencyContext)
        {
            switch (dependencyLoadPreference)
            {
                case DependencyLoadPreference.PreferDependencyContext:
                    return new PreferDependencyContextAssemblyLoadStrategy(pluginDependencyContext);
                case DependencyLoadPreference.PreferRemote:
                    return new PreferRemoteAssemblyLoadStrategy(pluginDependencyContext);
                case DependencyLoadPreference.PreferAppDomain:
                    return new PreferAppDomainAssemblyLoadStrategy(pluginDependencyContext);
            }

            throw new PrisePluginException($"Strategy {dependencyLoadPreference.ToString()} is not supported");
        }
    }
}
