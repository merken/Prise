using Prise.AssemblyScanning;
using System;

namespace Prise.Infrastructure
{
    public interface IPluginLoadOptions<T> : IDisposable
    {
        IAssemblyScanner<T> AssemblyScanner { get; }
        ISharedServicesProvider<T> SharedServicesProvider { get; }
        IRemotePluginActivator Activator { get; }
        IResultConverter ResultConverter { get; }
        IParameterConverter ParameterConverter { get; }
        IPluginAssemblyLoader<T> AssemblyLoader { get; }
        IProxyCreator<T> ProxyCreator { get; }
        IHostTypesProvider HostTypesProvider { get; }
        IRemoteTypesProvider<T> RemoteTypesProvider { get; }
        IRuntimePlatformContext RuntimePlatformContext { get; }
        IPluginSelector<T> PluginSelector { get; }
    }
}