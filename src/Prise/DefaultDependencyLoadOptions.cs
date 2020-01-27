using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Prise.Infrastructure;

namespace Prise
{
    [DebuggerDisplay("{DependencyLoadPreference.ToString()} - {NativeDependencyLoadPreference.ToString()}")]
    public class DefaultAssemblyLoadOptions<T> : IAssemblyLoadOptions<T>
    {
        private readonly PluginPlatformVersion pluginPlatformVersion;
        private readonly bool ignorePlatformInconsistencies;
        private readonly bool useCollectibleAssemblies;
        private readonly NativeDependencyLoadPreference nativeDependencyLoadPreference;

        public DefaultAssemblyLoadOptions(
            PluginPlatformVersion pluginPlatformVersion = null,
            bool ignorePlatformInconsistencies = false,
            bool useCollectibleAssemblies = true,
            NativeDependencyLoadPreference nativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferInstalledRuntime)
        {
            if (pluginPlatformVersion == null)
                this.pluginPlatformVersion = PluginPlatformVersion.Empty();
            else
                this.pluginPlatformVersion = pluginPlatformVersion;

            this.ignorePlatformInconsistencies = ignorePlatformInconsistencies;
            this.useCollectibleAssemblies = useCollectibleAssemblies;
            this.nativeDependencyLoadPreference = nativeDependencyLoadPreference;
        }

        public PluginPlatformVersion PluginPlatformVersion => this.pluginPlatformVersion;
        public bool IgnorePlatformInconsistencies => this.ignorePlatformInconsistencies;
        public bool UseCollectibleAssemblies => this.useCollectibleAssemblies;
        public NativeDependencyLoadPreference NativeDependencyLoadPreference => this.nativeDependencyLoadPreference;
    }
}
