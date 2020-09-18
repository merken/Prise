using System;
using System.Collections.Generic;

namespace Prise.AssemblyLoading
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
        IEnumerable<string> AdditionalProbingPaths { get; }
    }
}