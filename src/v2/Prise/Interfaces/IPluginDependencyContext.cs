using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Versioning;

namespace Prise.V2
{
    public interface IPluginDependencyContext : IDisposable
    {
        string FullPathToPluginAssembly { get; }
        IEnumerable<HostDependency> HostDependencies { get; }
        IEnumerable<RemoteDependency> RemoteDependencies { get; }
        IEnumerable<PluginDependency> PluginDependencies { get; }
        IEnumerable<PluginDependency> PluginReferenceDependencies { get; }
        IEnumerable<PluginResourceDependency> PluginResourceDependencies { get; }
        IEnumerable<PlatformDependency> PlatformDependencies { get; }
    }
}