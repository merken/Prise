using System;
using System.Collections.Generic;

namespace Prise.Core
{
    public interface IPluginLoadContext
    {
        string FullPathToPluginAssembly { get; }
        Type PluginType { get; }
        IEnumerable<Type> HostTypes { get; }
        IEnumerable<string> HostAssemblies { get; }
        IEnumerable<Type> DowngradableHostTypes { get; }
        IEnumerable<string> DowngradableHostAssemblies { get; }
        IEnumerable<Type> RemoteTypes { get; }
        NativeDependencyLoadPreference NativeDependencyLoadPreference { get; }
        PluginPlatformVersion PluginPlatformVersion { get; }
        IRuntimePlatformContext RuntimePlatformContext { get; }
        IEnumerable<string> AdditionalProbingPaths { get; }
        string HostFramework { get; }
        bool IgnorePlatformInconsistencies { get; }
    }
}