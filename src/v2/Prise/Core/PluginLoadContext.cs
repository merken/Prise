using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Versioning;

namespace Prise.Core
{
    public class PluginLoadContext : IPluginLoadContext
    {
        public PluginLoadContext(string fullPathToPluginAssembly, Type pluginType, string hostFramework)
        {
            this.FullPathToPluginAssembly = fullPathToPluginAssembly;
            this.HostTypes = new List<Type>() { typeof(Prise.Plugin.PluginAttribute), typeof(Microsoft.Extensions.DependencyInjection.ServiceCollection) };
            this.HostAssemblies = new List<string>();
            this.DowngradableTypes = new List<Type>() { typeof(Prise.Plugin.PluginAttribute) };
            this.DowngradableHostAssemblies = new List<string>();
            this.RemoteTypes = new List<Type>() { pluginType };
            this.NativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferInstalledRuntime;
            this.IgnorePlatformInconsistencies = false;
            this.HostFramework = hostFramework;
            this.RuntimePlatformContext = new DefaultRuntimePlatformContext();
            this.PluginPlatformVersion = PluginPlatformVersion.Empty();
        }

        public string FullPathToPluginAssembly { get; private set; }

        public IEnumerable<Type> HostTypes { get; private set; }

        public IEnumerable<string> HostAssemblies { get; private set; }

        public IEnumerable<Type> DowngradableTypes { get; private set; }

        public IEnumerable<string> DowngradableHostAssemblies { get; private set; }

        public IEnumerable<Type> RemoteTypes { get; private set; }

        public NativeDependencyLoadPreference NativeDependencyLoadPreference { get; private set; }

        public PluginPlatformVersion PluginPlatformVersion { get; private set; }

        public IRuntimePlatformContext RuntimePlatformContext { get; private set; }

        public string HostFramework { get; private set; }

        public bool IgnorePlatformInconsistencies { get; private set; }

        public static IPluginLoadContext DefaultPluginLoadContext(
            string fullPathToPluginAssembly,
            Type pluginType,
            IEnumerable<HostDependency> hostDependencies = null,
            IEnumerable<RemoteDependency> remoteDependencies = null,
            IEnumerable<PluginDependency> pluginDependencies = null,
            IEnumerable<PluginDependency> pluginReferenceDependencies = null,
            IEnumerable<PluginResourceDependency> pluginResourceDependencies = null,
            IEnumerable<PlatformDependency> platformDependencies = null,
            IRuntimePlatformContext runtimePlatformContext = null,
            bool ignorePlatformInconsistencies = false,
            string hostFramework = null)
        {
            hostFramework = Assembly
                    .GetEntryAssembly()?
                    .GetCustomAttribute<TargetFrameworkAttribute>()?
                    .FrameworkName;
            var ctx = new PluginLoadContext(fullPathToPluginAssembly, pluginType, hostFramework);
            ctx.IgnorePlatformInconsistencies = ignorePlatformInconsistencies;
            return ctx;
        }
    }
}