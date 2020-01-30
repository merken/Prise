using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Prise.AssemblyScanning;
using Prise.Plugin;

namespace Prise.Infrastructure
{
    public interface IPluginLogger<T> : IPluginLogger
    {
        void RemoteProxyCreated(T proxy);
        void PluginAssemblyDiscovered(AssemblyScanResult<T> scanResult);
        void PluginAssemblySelected(AssemblyScanResult<T> scanResult);
    }

    public interface IPluginLogger
    {
        void PluginContextCreated(IPluginLoadContext context);
        void PluginLoaded(Assembly assembly);
        void PluginTypeProvided(Type type);
        void PluginTypeSelected(Type type);
        void PluginActivationContextProvided(PluginActivationContext context);
        void RemoteBootstrapperActivated(object bootstrapper);
        void RemoteBootstrapperProxyCreated(IPluginBootstrapper bootstrapper);
        void RemoteInstanceCreated(object instance);
        void LoadReferenceAssembly(AssemblyName assemblyName);
        void LoadUnmanagedDll(string unmanagedDllName);
        void LoadReferenceFromAppDomainFailed(AssemblyName assemblyName);
        void IsHostAssembly(AssemblyName assemblyName);
        void IsRemoteAssembly(AssemblyName assemblyName);
        void LoadedFromAppDomain(AssemblyName assemblyName);
        void VersionMismatch(AssemblyName requested, AssemblyName hostAssembly);
        void LoadedFromDependencyContext(AssemblyName assemblyName, ValueOrProceed<Assembly> valueOrProceed);
        void LoadedFromRemote(AssemblyName assemblyName, ValueOrProceed<Assembly> valueOrProceed);
        void LoadedUnmanagedFromDependencyContext(string unmanagedDllName, ValueOrProceed<string> valueOrProceed);
        void LoadedUnmanagedFromAppDomain(string unmanagedDllName, ValueOrProceed<IntPtr> valueOrProceed);
        void LoadedUnmanagedFromRemote(string unmanagedDllName, ValueOrProceed<string> valueOrProceed);
    }
}
