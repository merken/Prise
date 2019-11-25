using Prise.AssemblyScanning;
using System;

namespace Prise.Infrastructure
{
    public interface IPluginLoadOptions<T> : IDisposable
    {
        IPluginPathProvider PluginPathProvider { get; }
        IAssemblyScanner<T> AssemblyScanner { get; }
        ISharedServicesProvider SharedServicesProvider { get; }
        IRemotePluginActivator Activator { get; }
        IResultConverter ResultConverter { get; }
        IParameterConverter ParameterConverter { get; }
        IPluginAssemblyLoader<T> AssemblyLoader { get; }
        IPluginAssemblyNameProvider PluginAssemblyNameProvider { get; }
        IProxyCreator<T> ProxyCreator { get; }
        IHostTypesProvider HostTypesProvider { get; }
        IRemoteTypesProvider RemoteTypesProvider { get; }
        IRuntimePlatformContext RuntimePlatformContext { get; }
        IPluginSelector PluginSelector { get; }
    }
}