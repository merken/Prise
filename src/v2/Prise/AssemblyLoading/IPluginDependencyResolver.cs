using System;
using System.Collections.Generic;
using System.IO;
using Prise.Core;

namespace Prise.AssemblyLoading
{
    public interface IPluginDependencyResolver : IDisposable
    {
        Stream ResolvePluginDependencyToPath(string fullPathToPluginAssembly, PluginDependency dependency, IEnumerable<string> additionalProbingPaths);
        string ResolvePlatformDependencyToPath(string fullPathToPluginAssembly, PlatformDependency dependency, IEnumerable<string> additionalProbingPaths);
        string ResolvePlatformDependencyPathToRuntime(PluginPlatformVersion pluginPlatformVersion, string platformDependencyPath);
    }
}