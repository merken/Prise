using System;
using System.Linq;
using System.Reflection;

namespace Prise.Infrastructure.NetCore
{
    internal abstract class AssemblyLoadStrategy
    {
        protected IPluginDependencyContext pluginDependencyContext;

        internal AssemblyLoadStrategy(IPluginDependencyContext pluginDependencyContext)
        {
            this.pluginDependencyContext = pluginDependencyContext;
        }

        protected bool IsHostAssembly(AssemblyName assemblyName) => this.pluginDependencyContext.HostDependencies.Any(h => h.DependencyName.Name == assemblyName.Name);
        protected bool IsRemoteAssembly(AssemblyName assemblyName) => this.pluginDependencyContext.RemoteDependencies.Any(r => r.DependencyName.Name == assemblyName.Name);

        protected ValueOrProceed<Assembly> TryLoadFromDependencyContext(AssemblyName assemblyName, Func<AssemblyName, ValueOrProceed<Assembly>> loadFromDependencyContext)
        {
            return loadFromDependencyContext(assemblyName);
        }

        protected ValueOrProceed<Assembly> TryLoadFromRemote(AssemblyName assemblyName, Func<AssemblyName, ValueOrProceed<Assembly>> loadFromRemote)
        {
            return loadFromRemote(assemblyName);
        }

        protected ValueOrProceed<Assembly> TryLoadFromAppDomain(AssemblyName assemblyName, Func<AssemblyName, ValueOrProceed<Assembly>> loadFromAppDomain)
        {
            return loadFromAppDomain(assemblyName);
        }
    }
}
