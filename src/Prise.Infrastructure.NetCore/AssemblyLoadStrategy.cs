using System;
using System.Reflection;

namespace Prise.Infrastructure.NetCore
{
    internal abstract class AssemblyLoadStrategy
    {
        protected Assembly TryLoadFromDependencyContext(AssemblyName assemblyName, Func<AssemblyName, Assembly> loadFromDependencyContext)
        {
            return loadFromDependencyContext(assemblyName);
        }

        protected Assembly TryLoadFromRemote(AssemblyName assemblyName, Func<AssemblyName, Assembly> loadFromRemote)
        {
            return loadFromRemote(assemblyName);
        }

        protected Assembly TryLoadFromAppDomain(AssemblyName assemblyName, Func<AssemblyName, Assembly> loadFromAppDomain)
        {
            return loadFromAppDomain(assemblyName);
        }
    }

    internal interface IAssemblyLoadStrategy
    {
        Assembly LoadAssembly(AssemblyName assemblyName,
            Func<AssemblyName, Assembly> loadFromDependencyContext,
            Func<AssemblyName, Assembly> loadFromRemote,
            Func<AssemblyName, Assembly> loadFromAppDomain);
    }

    internal class PreferDependencyContextAssemblyLoadStrategy : AssemblyLoadStrategy, IAssemblyLoadStrategy
    {
        public Assembly LoadAssembly(AssemblyName assemblyName,
            Func<AssemblyName, Assembly> loadFromDependencyContext,
            Func<AssemblyName, Assembly> loadFromRemote,
            Func<AssemblyName, Assembly> loadFromAppDomain)
        {
            var assembly = TryLoadFromDependencyContext(assemblyName, loadFromDependencyContext);

            if (assembly == null)
                assembly = TryLoadFromAppDomain(assemblyName, loadFromAppDomain);

            if (assembly == null)
                assembly = TryLoadFromRemote(assemblyName, loadFromRemote);

            return assembly;
        }
    }

    internal class PreferRemoteAssemblyLoadStrategy : AssemblyLoadStrategy, IAssemblyLoadStrategy
    {
        public Assembly LoadAssembly(AssemblyName assemblyName,
            Func<AssemblyName, Assembly> loadFromDependencyContext,
            Func<AssemblyName, Assembly> loadFromRemote,
            Func<AssemblyName, Assembly> loadFromAppDomain)
        {
            var assembly = TryLoadFromRemote(assemblyName, loadFromRemote);

            if (assembly == null)
                assembly = TryLoadFromDependencyContext(assemblyName, loadFromDependencyContext);

            if (assembly == null)
                assembly = TryLoadFromAppDomain(assemblyName, loadFromAppDomain);

            return assembly;
        }
    }

    internal class PreferAppDomainAssemblyLoadStrategy : AssemblyLoadStrategy, IAssemblyLoadStrategy
    {
        public Assembly LoadAssembly(AssemblyName assemblyName,
            Func<AssemblyName, Assembly> loadFromDependencyContext,
            Func<AssemblyName, Assembly> loadFromRemote,
            Func<AssemblyName, Assembly> loadFromAppDomain)
        {
            var assembly = TryLoadFromAppDomain(assemblyName, loadFromAppDomain);

            if (assembly == null)
                assembly = TryLoadFromDependencyContext(assemblyName, loadFromDependencyContext);

            if (assembly == null)
                assembly = TryLoadFromRemote(assemblyName, loadFromRemote);

            return assembly;
        }
    }

    internal static class AssemblyLoadStrategyFactory
    {
        internal static IAssemblyLoadStrategy GetAssemblyLoadStrategy(DependencyLoadPreference dependencyLoadPreference)
        {
            switch (dependencyLoadPreference)
            {
                case DependencyLoadPreference.PreferDependencyContext:
                    return new PreferDependencyContextAssemblyLoadStrategy();
                case DependencyLoadPreference.PreferRemote:
                    return new PreferRemoteAssemblyLoadStrategy();
                case DependencyLoadPreference.PreferAppDomain:
                    return new PreferAppDomainAssemblyLoadStrategy();
            }

            throw new NotSupportedException($"Strategy {dependencyLoadPreference.ToString()} is not supported");
        }
    }
}