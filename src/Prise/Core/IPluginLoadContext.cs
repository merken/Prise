using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

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
        IEnumerable<string> AdditionalProbingPaths { get; }
        IServiceCollection HostServices { get; }
        string HostFramework { get; }
        bool IgnorePlatformInconsistencies { get; }
    }
}