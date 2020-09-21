using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Prise.Core
{
    public class PluginLoadContext : IPluginLoadContext
    {
        public PluginLoadContext(string fullPathToPluginAssembly, Type pluginType, string hostFramework)
        {
            this.FullPathToPluginAssembly = fullPathToPluginAssembly;
            this.PluginType = pluginType;
            this.HostFramework = hostFramework;
            this.HostTypes = new List<Type>() { typeof(Prise.Plugin.PluginAttribute), typeof(Microsoft.Extensions.DependencyInjection.ServiceCollection) };
            this.HostAssemblies = new List<string>();
            this.DowngradableTypes = new List<Type>() { typeof(Prise.Plugin.PluginAttribute) };
            this.DowngradableHostAssemblies = new List<string>();
            this.RemoteTypes = new List<Type>() { pluginType };
            this.NativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferInstalledRuntime;
            this.IgnorePlatformInconsistencies = false;
            this.RuntimePlatformContext = null;
            this.PluginPlatformVersion = PluginPlatformVersion.Empty();
        }

        public string FullPathToPluginAssembly { get; set; }

        public Type PluginType { get; set; }

        public IEnumerable<Type> HostTypes { get; set; }

        public IEnumerable<string> HostAssemblies { get; set; }

        public IEnumerable<Type> DowngradableTypes { get; set; }

        public IEnumerable<string> DowngradableHostAssemblies { get; set; }

        public IEnumerable<Type> RemoteTypes { get; set; }

        public NativeDependencyLoadPreference NativeDependencyLoadPreference { get; set; }

        public PluginPlatformVersion PluginPlatformVersion { get; set; }

        public IRuntimePlatformContext RuntimePlatformContext { get; set; }

        public IEnumerable<string> AdditionalProbingPaths { get; set; }

        public bool IgnorePlatformInconsistencies { get; set; }

        public string HostFramework { get; set; }

        public static PluginLoadContext DefaultPluginLoadContext(string fullPathToPluginAssembly,
                                                                  Type pluginType,
                                                                  string hostFramework)
            => new PluginLoadContext(fullPathToPluginAssembly, pluginType, hostFramework);
    }
}