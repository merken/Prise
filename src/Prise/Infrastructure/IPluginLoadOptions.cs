using Prise.Proxy;
using Prise.AssemblyScanning;
using System;

namespace Prise.Infrastructure
{
    public interface IPluginLoadOptions<T> : IDisposable
    {
        IPluginLogger<T> Logger { get; }
        IAssemblyScanner<T> AssemblyScanner { get; }
        IPluginAssemblyLoader<T> AssemblyLoader { get; }
        ISharedServicesProvider<T> SharedServicesProvider { get; }
        IPluginTypesProvider<T> PluginTypesProvider { get; }
        IPluginActivationContextProvider<T> PluginActivationContextProvider { get; }
        IRemotePluginActivator Activator { get; }
        IResultConverter ResultConverter { get; }
        IParameterConverter ParameterConverter { get; }
        IPluginProxyCreator<T> ProxyCreator { get; }
        IHostTypesProvider HostTypesProvider { get; }
        IRemoteTypesProvider<T> RemoteTypesProvider { get; }
        IRuntimePlatformContext RuntimePlatformContext { get; }
        IAssemblySelector<T> AssemblySelector { get; }
        IPluginSelector<T> PluginSelector { get; }
    }
}