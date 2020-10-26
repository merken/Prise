using System;
using System.Collections.Generic;
using System.IO;


namespace Prise.AssemblyLoading
{
    public interface IPluginDependencyResolver : IDisposable
    {
        Stream ResolvePluginDependencyToPath(string initialPluginLoadDirectory, PluginDependency dependency, IEnumerable<string> additionalProbingPaths);
        string ResolvePlatformDependencyToPath(string initialPluginLoadDirectory, PlatformDependency dependency, IEnumerable<string> additionalProbingPaths);
        string ResolvePlatformDependencyPathToRuntime(PluginPlatformVersion pluginPlatformVersion, string platformDependencyPath);
    }
}