using System;
using System.Collections.Generic;

namespace Prise.Infrastructure
{
    public interface IPluginDependencyContext : IDisposable
    {
        IPluginLoadContext PluginLoadContext { get; }
        IEnumerable<HostDependency> HostDependencies { get; }
        IEnumerable<RemoteDependency> RemoteDependencies { get; }
        IEnumerable<PluginDependency> PluginDependencies { get; }
        IEnumerable<PluginDependency> PluginReferenceDependencies { get; }
        IEnumerable<PluginResourceDependency> PluginResourceDependencies { get; }
        IEnumerable<PlatformDependency> PlatformDependencies { get; }
    }
}
