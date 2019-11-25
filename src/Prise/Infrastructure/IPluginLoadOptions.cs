using Prise.AssemblyScanning;
using System;

namespace Prise.Infrastructure
{
    public interface IPluginLoadOptions<T> : IDisposable
    {
        IAssemblyScanner<T> AssemblyScanner { get; }
        ISharedServicesProvider SharedServicesProvider { get; }
        IRemotePluginActivator Activator { get; }
        IResultConverter ResultConverter { get; }
        IParameterConverter ParameterConverter { get; }
        IPluginAssemblyLoader<T> AssemblyLoader { get; }
        IProxyCreator<T> ProxyCreator { get; }
        IHostTypesProvider HostTypesProvider { get; }
        IRemoteTypesProvider RemoteTypesProvider { get; }
        IRuntimePlatformContext RuntimePlatformContext { get; }
        IPluginSelector PluginSelector { get; }
    }
}