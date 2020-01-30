using System;
using System.Reflection;
using Prise.AssemblyScanning;
using Prise.Infrastructure;
using Prise.Plugin;

namespace Prise
{
    public class NullPluginLogger<T> : PluginLoggerBase<T>
    {
        protected override void Log(string message)
        {
            // Nothing is done in this logger
        }
    }

    public class ConsolePluginLogger<T> : PluginLoggerBase<T>
    {
        protected override void Log(string message)
        {
            Console.WriteLine(message);
        }
    }

    public abstract class PluginLoggerBase<T> : IPluginLogger<T>
    {
        private string pluginType = typeof(T).Name;

        protected abstract void Log(string message);

        public void LoadReferenceFromAppDomainFailed(AssemblyName assemblyName)
        {
            Log($"Plugin<{this.pluginType}> assembly {assemblyName.Name} could not be loaded from AssemblyLoadContext.Default.LoadFromAssemblyName {assemblyName.Name}. This is possibly a platform assemly.");
        }

        public void IsHostAssembly(AssemblyName assemblyName)
        {
            Log($"Plugin<{this.pluginType}> assembly {assemblyName.Name} was registered as a Host type");
        }

        public void IsRemoteAssembly(AssemblyName assemblyName)
        {
            Log($"Plugin<{this.pluginType}> assembly {assemblyName.Name} was registered as a Remote type");
        }

        public void LoadedFromAppDomain(AssemblyName assemblyName)
        {
            Log($"Plugin<{this.pluginType}> assembly {assemblyName.Name} was loaded from the ApplicationLoadContext.Default");
        }

        public void LoadedFromDependencyContext(AssemblyName assemblyName, ValueOrProceed<Assembly> valueOrProceed)
        {
            if (valueOrProceed.Value != null)
                Log($"Plugin<{this.pluginType}> assembly {assemblyName.Name} was loaded from the Dependency Context");
        }

        public void LoadedFromRemote(AssemblyName assemblyName, ValueOrProceed<Assembly> valueOrProceed)
        {
            if (valueOrProceed.Value != null)
                Log($"Plugin<{this.pluginType}> assembly {assemblyName.Name} was loaded from the Remote Location");
        }

        public void LoadedUnmanagedFromAppDomain(string unmanagedDllName, ValueOrProceed<IntPtr> valueOrProceed)
        {
            if (valueOrProceed.Value != IntPtr.Zero)
                Log($"Plugin<{this.pluginType}> unmananged assembly {unmanagedDllName} was loaded from the ApplicationLoadContext.Default");
        }

        public void LoadedUnmanagedFromDependencyContext(string unmanagedDllName, ValueOrProceed<string> valueOrProceed)
        {
            if (valueOrProceed.Value != null)
                Log($"Plugin<{this.pluginType}> unmananged assembly {unmanagedDllName} was loaded from the Dependency Context");
        }

        public void LoadedUnmanagedFromRemote(string unmanagedDllName, ValueOrProceed<string> valueOrProceed)
        {
            if (valueOrProceed.Value != null)
                Log($"Plugin<{this.pluginType}> unmananged assembly {unmanagedDllName} was loaded from the Remote Location");
        }

        public void LoadReferenceAssembly(AssemblyName assemblyName)
        {
            Log($"Plugin<{this.pluginType}> Loading referenced assembly {assemblyName.Name}");
        }

        public void LoadUnmanagedDll(string unmanagedDllName)
        {
            Log($"Plugin<{this.pluginType}> Loading unmanaged referenced assembly {unmanagedDllName}");
        }

        public void PluginActivationContextProvided(PluginActivationContext context)
        {
            Log($"Plugin<{this.pluginType}> of implementation type {context.PluginType.Name} activated from assembly {context.PluginAssembly.GetName().Name}");
        }

        public void PluginAssemblyDiscovered(AssemblyScanResult<T> scanResult)
        {
            Log($"Plugin<{this.pluginType}> was discovered in assembly {scanResult.AssemblyName} at {scanResult.AssemblyPath}");
        }

        public void PluginAssemblySelected(AssemblyScanResult<T> scanResult)
        {
            Log($"Plugin<{this.pluginType}> was selected in assembly {scanResult.AssemblyName} at {scanResult.AssemblyPath}");
        }

        public void PluginContextCreated(IPluginLoadContext context)
        {
            Log($"PluginLoadContext<{this.pluginType}> was created for assembly {context.PluginAssemblyName} at {context.PluginAssemblyPath}");
        }

        public void PluginLoaded(Assembly assembly)
        {
            Log($"Plugin<{this.pluginType}> was loaded from assembly {assembly.GetName().Name}");
        }

        public void PluginTypeProvided(Type type)
        {
            Log($"Plugin<{this.pluginType}> type was provided {type.Name}");
        }

        public void PluginTypeSelected(Type type)
        {
            Log($"Plugin<{this.pluginType}> type was selected {type.Name}");
        }

        public void RemoteBootstrapperActivated(object bootstrapper)
        {
            Log($"Plugin<{this.pluginType}> bootstrapper was activated {bootstrapper.GetType().Name}");
        }

        public void RemoteBootstrapperProxyCreated(IPluginBootstrapper bootstrapper)
        {
            Log($"Plugin<{this.pluginType}> bootstrapper proxy was created {bootstrapper.GetType().Name}");
        }

        public void RemoteInstanceCreated(object instance)
        {
            Log($"Plugin<{this.pluginType}> instance was created {instance.GetType().Name}");
        }

        public void RemoteProxyCreated(T proxy)
        {
            Log($"Plugin<{this.pluginType}> proxy was created {proxy.GetType().Name}");
        }

        public void VersionMismatch(AssemblyName requested, AssemblyName hostAssembly)
        {
            Log($"Plugin<{this.pluginType}> Assembly reference {requested.Name} with version {requested.Version} was requested but not found in the host. The version from the host is {hostAssembly.Version}. Possible version mismatch. Please downgrade your plugin.");
        }
    }
}
