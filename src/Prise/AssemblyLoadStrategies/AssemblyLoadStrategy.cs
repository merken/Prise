using System;
using System.Linq;
using System.Reflection;
using Prise.Infrastructure;

namespace Prise
{
    internal abstract class AssemblyLoadStrategy
    {
        protected IPluginLoadContext pluginLoadContext;
        protected IPluginDependencyContext pluginDependencyContext;

        internal AssemblyLoadStrategy(IPluginLoadContext pluginLoadContext, IPluginDependencyContext pluginDependencyContext)
        {
            this.pluginLoadContext = pluginLoadContext;
            this.pluginDependencyContext = pluginDependencyContext;
        }

        protected bool IsHostAssembly(AssemblyName assemblyName) => this.pluginDependencyContext.HostDependencies.Any(h => h.DependencyName.Name == assemblyName.Name);
        protected bool IsRemoteAssembly(AssemblyName assemblyName) => this.pluginDependencyContext.RemoteDependencies.Any(r => r.DependencyName.Name == assemblyName.Name);

        protected ValueOrProceed<Assembly> TryLoadFromDependencyContext(AssemblyName assemblyName, Func<IPluginLoadContext, AssemblyName, ValueOrProceed<Assembly>> loadFromDependencyContext)
        {
            return loadFromDependencyContext(this.pluginLoadContext, assemblyName);
        }

        protected ValueOrProceed<Assembly> TryLoadFromRemote(AssemblyName assemblyName, Func<IPluginLoadContext, AssemblyName, ValueOrProceed<Assembly>> loadFromRemote)
        {
            return loadFromRemote(this.pluginLoadContext, assemblyName);
        }

        protected ValueOrProceed<Assembly> TryLoadFromAppDomain(AssemblyName assemblyName, Func<IPluginLoadContext, AssemblyName, ValueOrProceed<Assembly>> loadFromAppDomain)
        {
            return loadFromAppDomain(this.pluginLoadContext, assemblyName);
        }
    }
}
