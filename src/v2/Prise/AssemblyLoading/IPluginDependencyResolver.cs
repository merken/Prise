using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Versioning;

namespace Prise.AssemblyLoading
{

    public interface IPluginDependencyResolver : IDisposable
    {
        Stream ResolvePluginDependencyToPath(string fullPathToPluginAssembly, PluginDependency dependency);
        string ResolvePlatformDependencyToPath(string fullPathToPluginAssembly, PlatformDependency dependency);
        string ResolvePlatformDependencyPathToRuntime(PluginPlatformVersion pluginPlatformVersion, string platformDependencyPath);
    }
}