using System;
using System.Linq;
using System.Reflection;
using Prise.Infrastructure;
using Prise.Util;

namespace Prise
{
    /// <summary>
    /// This is the default assembly load strategy for any requested assembly.
    /// For Managed Assemblies, this is the pipeline:
    ///     Firstly, the assembly is loaded from the App Domain, keeping into account that a requested assembly is a host assemly and not explicitly setup to be loaded from the remote.
    ///     Secondly, the assembly is loaded from the plugin dependency context. If the plugin requires this assembly based on the deps.json file.
    ///     Thirdly, the assembly is loaded from the remote location
    ///     The loading strategy is a pipeline, if the return value is allowed to proceed, it will continue, otherwise it will exit the pipe.
    /// For Unmanaged Assemblies, this is the pipeline:
    ///     Firstly, Load the assembly from the plugin dependency context
    ///     Secondly, load the assembly from the host app domain
    ///     Thirdly, load the assembly from the plugin remote location
    /// </summary>
    public class DefaultAssemblyLoadStrategy : IAssemblyLoadStrategy
    {
        protected IPluginLogger logger;
        protected IPluginLoadContext pluginLoadContext;
        protected IPluginDependencyContext pluginDependencyContext;

        public DefaultAssemblyLoadStrategy(
            IPluginLogger logger,
            IPluginLoadContext pluginLoadContext,
            IPluginDependencyContext pluginDependencyContext)
        {
            this.logger = logger.ThrowIfNull(nameof(logger));
            this.pluginLoadContext = pluginLoadContext.ThrowIfNull(nameof(pluginLoadContext));
            this.pluginDependencyContext = pluginDependencyContext.ThrowIfNull(nameof(pluginDependencyContext));
        }

        public virtual Assembly LoadAssembly(AssemblyName assemblyName,
            Func<IPluginLoadContext, AssemblyName, ValueOrProceed<Assembly>> loadFromDependencyContext,
            Func<IPluginLoadContext, AssemblyName, ValueOrProceed<Assembly>> loadFromRemote,
            Func<IPluginLoadContext, AssemblyName, ValueOrProceed<Assembly>> loadFromAppDomain)
        {
            if (assemblyName.Name == null)
                return null;

            ValueOrProceed<Assembly> valueOrProceed = ValueOrProceed<Assembly>.FromValue(null, true);

            var isHostAssembly = IsHostAssembly(assemblyName);
            var isRemoteAssembly = IsRemoteAssembly(assemblyName);

            if (isHostAssembly)
                this.logger.IsHostAssembly(assemblyName);
            if (isRemoteAssembly)
                this.logger.IsRemoteAssembly(assemblyName);

            if (isHostAssembly && !isRemoteAssembly) // Load from Default App Domain (host)
            {
                valueOrProceed = loadFromAppDomain(this.pluginLoadContext, assemblyName);
                if (valueOrProceed.Value != null)
                {
                    this.logger.LoadedFromAppDomain(assemblyName);
                    return null; // fallback to default loading mechanism
                }
            }

            if (valueOrProceed.CanProceed)
            {
                valueOrProceed = loadFromDependencyContext(this.pluginLoadContext, assemblyName);
                this.logger.LoadedFromDependencyContext(assemblyName, valueOrProceed);
            }

            if (valueOrProceed.CanProceed)
            {
                valueOrProceed = loadFromRemote(this.pluginLoadContext, assemblyName);
                this.logger.LoadedFromRemote(assemblyName, valueOrProceed);
            }

            return valueOrProceed.Value;
        }

        public virtual NativeAssembly LoadUnmanagedDll(string unmanagedDllName,
            Func<IPluginLoadContext, string, ValueOrProceed<string>> loadFromDependencyContext,
            Func<IPluginLoadContext, string, ValueOrProceed<string>> loadFromRemote,
            Func<IPluginLoadContext, string, ValueOrProceed<IntPtr>> loadFromAppDomain)
        {
            ValueOrProceed<string> valueOrProceed = ValueOrProceed<string>.FromValue(String.Empty, true);
            ValueOrProceed<IntPtr> ptrValueOrProceed = ValueOrProceed<IntPtr>.FromValue(IntPtr.Zero, true);

            valueOrProceed = loadFromDependencyContext(this.pluginLoadContext, unmanagedDllName);
            this.logger.LoadedUnmanagedFromDependencyContext(unmanagedDllName, valueOrProceed);

            if (valueOrProceed.CanProceed)
            {
                ptrValueOrProceed = loadFromAppDomain(this.pluginLoadContext, unmanagedDllName);
                this.logger.LoadedUnmanagedFromAppDomain(unmanagedDllName, ptrValueOrProceed);
            }

            if (valueOrProceed.CanProceed && ptrValueOrProceed.CanProceed)
            {
                valueOrProceed = loadFromRemote(this.pluginLoadContext, unmanagedDllName);
                this.logger.LoadedUnmanagedFromRemote(unmanagedDllName, valueOrProceed);
            }

            return NativeAssembly.Create(valueOrProceed.Value, ptrValueOrProceed.Value);
        }

        protected virtual bool IsHostAssembly(AssemblyName assemblyName) => this.pluginDependencyContext.HostDependencies.Any(h => h.DependencyName.Name == assemblyName.Name);
        protected virtual bool IsRemoteAssembly(AssemblyName assemblyName) => this.pluginDependencyContext.RemoteDependencies.Any(r => r.DependencyName.Name == assemblyName.Name);
    }
}
