using System;
using System.Diagnostics;

namespace Prise.Infrastructure
{
    public class PluginPlatformVersion
    {
        public string Version { get; private set; }
        public RuntimeType Runtime { get; private set; }
        public bool IsSpecified => !String.IsNullOrEmpty(Version);

        public static PluginPlatformVersion Create(string version, RuntimeType runtime = RuntimeType.UnSpecified) => new PluginPlatformVersion
        {
            Version = version,
            Runtime = runtime
        };

        public static PluginPlatformVersion Empty() => new PluginPlatformVersion();
    }

    public interface IAssemblyLoadOptions
    {
        PluginPlatformVersion PluginPlatformVersion { get; }
        DependencyLoadPreference DependencyLoadPreference { get; }
        NativeDependencyLoadPreference NativeDependencyLoadPreference { get; }
    }

    [DebuggerDisplay("{DependencyLoadPreference.ToString()} - {NativeDependencyLoadPreference.ToString()")]

    public class AssemblyLoadOptions : IAssemblyLoadOptions
    {
        private readonly PluginPlatformVersion pluginPlatformVersion;
        private readonly DependencyLoadPreference dependencyLoadPreference;
        private readonly NativeDependencyLoadPreference nativeDependencyLoadPreference;
        public AssemblyLoadOptions(
            PluginPlatformVersion pluginPlatformVersion,
            DependencyLoadPreference dependencyLoadPreference = DependencyLoadPreference.PreferRemote,
            NativeDependencyLoadPreference nativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferInstalledRuntime)
        {
            this.pluginPlatformVersion = pluginPlatformVersion;
            this.dependencyLoadPreference = dependencyLoadPreference;
            this.nativeDependencyLoadPreference = nativeDependencyLoadPreference;
        }

        public PluginPlatformVersion PluginPlatformVersion => this.pluginPlatformVersion;
        public DependencyLoadPreference DependencyLoadPreference => this.dependencyLoadPreference;
        public NativeDependencyLoadPreference NativeDependencyLoadPreference => this.nativeDependencyLoadPreference;
    }
}