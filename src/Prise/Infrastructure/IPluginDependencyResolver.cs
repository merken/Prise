using System;
using System.Collections.Generic;
using System.IO;

namespace Prise.Infrastructure
{
    public interface IPluginDependencyResolver<T> : IDisposable
    {
        Stream ResolvePluginDependencyToPath(string dependencyPath, IEnumerable<string> probingPaths, PluginDependency dependency);
        string ResolvePlatformDependencyToPath(string dependencyPath, IEnumerable<string> probingPaths, PlatformDependency dependency);
        string ResolvePlatformDependencyPathToRuntime(PluginPlatformVersion pluginPlatformVersion, string platformDependencyPath);
    }
}
