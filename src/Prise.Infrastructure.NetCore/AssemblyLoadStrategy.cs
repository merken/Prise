using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace Prise.Infrastructure.NetCore
{
    internal interface IAssemblyLoadStrategy
    {
        Assembly LoadAssembly(AssemblyName assemblyName, Func<AssemblyName, Assembly> loadFromRemote);
    }

    internal abstract class AssemblyLoadStrategy
    {
        protected Assembly TryLoadFromDependencyContext(AssemblyName assemblyName)
        {
            var defaultDependencies = DependencyContext.Default;
            var candidateAssembly = defaultDependencies.CompileLibraries.FirstOrDefault(d => String.Compare(d.Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase) == 0);
            if (candidateAssembly != null)
            {
                return Assembly.Load(new AssemblyName(candidateAssembly.Name));
            }
            return null;
        }

        protected Assembly TryLoadFromRemote(AssemblyName assemblyName, Func<AssemblyName, Assembly> loadFromRemote)
        {
            return loadFromRemote(assemblyName);
        }

        protected Assembly TryLoadFromAppDomain(AssemblyName assemblyName)
        {
            return Assembly.Load(assemblyName);
        }
    }

    internal class PreferDependencyContextAssemblyLoadStrategy : AssemblyLoadStrategy, IAssemblyLoadStrategy
    {
        public Assembly LoadAssembly(AssemblyName assemblyName, Func<AssemblyName, Assembly> loadFromRemote)
        {
            var assembly = TryLoadFromDependencyContext(assemblyName);

            if (assembly == null)
                assembly = TryLoadFromRemote(assemblyName, loadFromRemote);

            if (assembly == null)
                assembly = TryLoadFromAppDomain(assemblyName);

            return assembly;
        }
    }

    internal class PreferRemoteAssemblyLoadStrategy : AssemblyLoadStrategy, IAssemblyLoadStrategy
    {
        public Assembly LoadAssembly(AssemblyName assemblyName, Func<AssemblyName, Assembly> loadFromRemote)
        {
            var assembly = TryLoadFromRemote(assemblyName, loadFromRemote);

            if (assembly == null)
                assembly = TryLoadFromDependencyContext(assemblyName);

            if (assembly == null)
                assembly = TryLoadFromAppDomain(assemblyName);

            return assembly;
        }
    }

    internal class PreferAppDomainAssemblyLoadStrategy : AssemblyLoadStrategy, IAssemblyLoadStrategy
    {
        public Assembly LoadAssembly(AssemblyName assemblyName, Func<AssemblyName, Assembly> loadFromRemote)
        {
            var assembly = TryLoadFromAppDomain(assemblyName);

            if (assembly == null)
                assembly = TryLoadFromDependencyContext(assemblyName);

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