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

    public interface IAssemblyLoadOptions<T>
    {
        PluginPlatformVersion PluginPlatformVersion { get; }
        bool IgnorePlatformInconsistencies { get; }
        NativeDependencyLoadPreference NativeDependencyLoadPreference { get; }
    }

    [DebuggerDisplay("{DependencyLoadPreference.ToString()} - {NativeDependencyLoadPreference.ToString()}")]
    public class DefaultAssemblyLoadOptions<T> : IAssemblyLoadOptions<T>
    {
        private readonly PluginPlatformVersion pluginPlatformVersion;
        private readonly bool ignorePlatformInconsistencies;
        private readonly NativeDependencyLoadPreference nativeDependencyLoadPreference;
        public DefaultAssemblyLoadOptions(
            PluginPlatformVersion pluginPlatformVersion,
            bool ignorePlatformInconsistencies = false,
            NativeDependencyLoadPreference nativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferInstalledRuntime)
        {
            if (pluginPlatformVersion == null)
                this.pluginPlatformVersion = PluginPlatformVersion.Empty();
            else
                this.pluginPlatformVersion = pluginPlatformVersion;

            this.ignorePlatformInconsistencies = ignorePlatformInconsistencies;
            this.nativeDependencyLoadPreference = nativeDependencyLoadPreference;
        }

        public PluginPlatformVersion PluginPlatformVersion => this.pluginPlatformVersion;
        public bool IgnorePlatformInconsistencies => this.ignorePlatformInconsistencies;
        public NativeDependencyLoadPreference NativeDependencyLoadPreference => this.nativeDependencyLoadPreference;
    }
}