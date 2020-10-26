using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Prise.Utils;

namespace Prise
{
    public class PluginLoadContext : IPluginLoadContext
    {
        public PluginLoadContext(string fullPathToPluginAssembly, Type pluginType, string hostFramework)
        {
            this.FullPathToPluginAssembly = fullPathToPluginAssembly.ThrowIfNullOrEmpty(nameof(pluginType));
            this.PluginType = pluginType.ThrowIfNull(nameof(pluginType));
            this.HostFramework = hostFramework.ThrowIfNullOrEmpty(nameof(hostFramework));
            this.HostTypes = new List<Type>() { typeof(Prise.Plugin.PluginAttribute), typeof(Microsoft.Extensions.DependencyInjection.ServiceCollection) };
            this.HostAssemblies = new List<string>();
            this.DowngradableHostTypes = new List<Type>() { typeof(Prise.Plugin.PluginAttribute) };
            this.DowngradableHostAssemblies = new List<string>();
            this.RemoteTypes = new List<Type>() { pluginType };
            this.NativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferInstalledRuntime;
            this.IgnorePlatformInconsistencies = false;
            this.PluginPlatformVersion = PluginPlatformVersion.Empty();
            this.HostServices = new ServiceCollection();
        }

        public string FullPathToPluginAssembly { get; set; }

        public Type PluginType { get; set; }

        public IEnumerable<Type> HostTypes { get; set; }

        public IEnumerable<string> HostAssemblies { get; set; }

        public IEnumerable<Type> DowngradableHostTypes { get; set; }

        public IEnumerable<string> DowngradableHostAssemblies { get; set; }

        public IEnumerable<Type> RemoteTypes { get; set; }

        public NativeDependencyLoadPreference NativeDependencyLoadPreference { get; set; }

        public PluginPlatformVersion PluginPlatformVersion { get; set; }

        public IRuntimePlatformContext RuntimePlatformContext { get; set; }

        public IEnumerable<string> AdditionalProbingPaths { get; set; }

        public bool IgnorePlatformInconsistencies { get; set; }

        public string HostFramework { get; set; }

        public IServiceCollection HostServices { get; }

        public static PluginLoadContext DefaultPluginLoadContext(string fullPathToPluginAssembly,
                                                                  Type pluginType,
                                                                  string hostFramework)
            => new PluginLoadContext(fullPathToPluginAssembly, pluginType, hostFramework);
    }
}