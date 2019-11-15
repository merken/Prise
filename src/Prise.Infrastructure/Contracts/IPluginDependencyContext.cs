using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Prise.Infrastructure
{
    [DebuggerDisplay("{DependencyName.Name}")]
    public class HostDependency
    {
        public AssemblyName DependencyName { get; set; }
    }

    [DebuggerDisplay("{DependencyName.Name}")]
    public class RemoteDependency
    {
        public AssemblyName DependencyName { get; set; }
    }

    [DebuggerDisplay("{DependencyNameWithoutExtension}")]
    public class PluginDependency
    {
        public string DependencyNameWithoutExtension { get; set; }
        public string Version { get; set; }
        public string DependencyPath { get; set; }
        public string ProbingPath { get; set; }
    }

    [DebuggerDisplay("{Path}")]
    public class PluginResourceDependency
    {
        public string Path { get; set; }
    }

    [DebuggerDisplay("{DependencyNameWithoutExtension}")]
    public class PlatformDependency
    {
        public string DependencyNameWithoutExtension { get; set; }
        public string Version { get; set; }
        public string DependencyPath { get; set; }
        public string ProbingPath { get; set; }
    }

    public enum RuntimeType
    {
        UnSpecified = 0,
        AspNetCoreAll,
        AspNetCoreApp,
        NetCoreApp,
        WindowsDesktopApp,
    }

    [DebuggerDisplay("{RuntimeType.ToString()}-{Version}-{Location}")]
    public class Runtime
    {
        public RuntimeType RuntimeType { get; set; }
        public string Version { get; set; }
        public string Location { get; set; }
    }

    public class RuntimeInformation
    {
        [DebuggerDisplay("{Runtimes?.Count()}")]
        public IEnumerable<Runtime> Runtimes { get; set; }
    }

    public interface IPluginDependencyContext : IDisposable
    {
        AssemblyName Plugin { get; }
        IEnumerable<HostDependency> HostDependencies { get; }
        IEnumerable<RemoteDependency> RemoteDependencies { get; }
        IEnumerable<PluginDependency> PluginDependencies { get; }
        IEnumerable<PluginDependency> PluginReferenceDependencies { get; }
        IEnumerable<PluginResourceDependency> PluginResourceDependencies { get; }
        IEnumerable<PlatformDependency> PlatformDependencies { get; }
    }

    public interface IPluginDependencyResolver : IDisposable
    {
        Stream ResolvePluginDependencyToPath(string dependencyPath, IEnumerable<string> probingPaths, PluginDependency dependency);
        string ResolvePlatformDependencyToPath(string dependencyPath, IEnumerable<string> probingPaths, PlatformDependency dependency);
        string ResolvePlatformDependencyPathToRuntime(PluginPlatformVersion pluginPlatformVersion, string platformDependencyPath);
    }
}
