using System;

namespace Prise.Infrastructure
{
    public interface IPluginLoadOptions<T> : IDisposable
    {
        IRootPathProvider RootPathProvider { get; }
        ISharedServicesProvider SharedServicesProvider { get; }
        IRemotePluginActivator Activator { get; }
        IResultConverter ResultConverter { get; }
        IParameterConverter ParameterConverter { get; }
        IPluginAssemblyLoader<T> AssemblyLoader { get; }
        IPluginAssemblyNameProvider PluginAssemblyNameProvider { get; }
        IProxyCreator<T> ProxyCreator { get; }
    }
}