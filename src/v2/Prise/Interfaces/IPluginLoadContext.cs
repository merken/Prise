using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;

namespace Prise.V2
{
    public interface IPluginLoadContext
    {
        string FullPathToPluginAssembly { get; }
        IEnumerable<Type> HostTypes { get; }
        IEnumerable<string> HostAssemblies { get; }
        IEnumerable<Type> DowngradableTypes { get; }
        IEnumerable<string> DowngradableHostAssemblies { get; }
        IEnumerable<Type> RemoteTypes { get; }
        NativeDependencyLoadPreference NativeDependencyLoadPreference { get; }
        PluginPlatformVersion PluginPlatformVersion { get; }
        IRuntimePlatformContext RuntimePlatformContext { get; }
        string HostFramework { get; }
        bool IgnorePlatformInconsistencies { get; }
    }
}