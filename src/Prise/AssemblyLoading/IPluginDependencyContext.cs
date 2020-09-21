using System;
using System.Collections.Generic;

namespace Prise.AssemblyLoading
{
    public interface IPluginDependencyContext : IDisposable
    {
        string FullPathToPluginAssembly { get; }
        /// <summary>
        /// Host dependencies are detected automatically by reading out the deps.json file
        /// </summary>
        /// <value></value>
        IEnumerable<HostDependency> HostDependencies { get; }
        /// <summary>
        /// Remote dependencies are specified manually via the AddRemoteType builder
        /// </summary>
        /// <value></value>
        IEnumerable<RemoteDependency> RemoteDependencies { get; }
        /// <summary>
        /// Plugin dependencies are detected automatically by reading out the deps.json file
        /// </summary>
        /// <value></value>
        IEnumerable<PluginDependency> PluginDependencies { get; }
        IEnumerable<PluginResourceDependency> PluginResourceDependencies { get; }
        IEnumerable<PlatformDependency> PlatformDependencies { get; }
        IEnumerable<string> AdditionalProbingPaths { get; }
    }
}