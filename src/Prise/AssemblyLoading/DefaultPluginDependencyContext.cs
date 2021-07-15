using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Prise.Utils;

namespace Prise.AssemblyLoading
{
    public class DefaultPluginDependencyContext : IPluginDependencyContext
    {
        public string FullPathToPluginAssembly { get; set; }
        public IEnumerable<HostDependency> HostDependencies { get; set; }
        public IEnumerable<RemoteDependency> RemoteDependencies { get; set; }
        public IEnumerable<PluginDependency> PluginDependencies { get; set; }
        public IEnumerable<PluginResourceDependency> PluginResourceDependencies { get; set; }
        public IEnumerable<PlatformDependency> PlatformDependencies { get; set; }
        public IEnumerable<string> AdditionalProbingPaths { get; set; }
        internal DefaultPluginDependencyContext(string fullPathToPluginAssembly,
                                               IEnumerable<HostDependency> hostDependencies,
                                               IEnumerable<RemoteDependency> remoteDependencies,
                                               IEnumerable<PluginDependency> pluginDependencies,
                                               IEnumerable<PluginResourceDependency> pluginResourceDependencies,
                                               IEnumerable<PlatformDependency> platformDependencies,
                                               IEnumerable<string> additionalProbingPaths)
        {
            this.FullPathToPluginAssembly = fullPathToPluginAssembly.ThrowIfNull(nameof(fullPathToPluginAssembly));
            this.HostDependencies = hostDependencies.ThrowIfNull(nameof(hostDependencies));
            this.RemoteDependencies = remoteDependencies.ThrowIfNull(nameof(remoteDependencies));
            this.PluginDependencies = pluginDependencies.ThrowIfNull(nameof(pluginDependencies));
            this.PluginResourceDependencies = pluginResourceDependencies.ThrowIfNull(nameof(pluginResourceDependencies));
            this.PlatformDependencies = platformDependencies.ThrowIfNull(nameof(platformDependencies));
            this.AdditionalProbingPaths = additionalProbingPaths ?? Enumerable.Empty<string>();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Dependency context for plugin: {this.FullPathToPluginAssembly}");

            builder.AppendLine($"HostDependencies");
            foreach (var p in this.HostDependencies)
                builder.AppendLine($"{p.DependencyName.Name} {p.DependencyName.Version}");

            builder.AppendLine($"");
            builder.AppendLine($"RemoteDependencies");
            foreach (var p in this.RemoteDependencies)
                builder.AppendLine($"{p.DependencyName.Name} {p.DependencyName.Version}");

            builder.AppendLine($"");
            builder.AppendLine($"PlatformDependencies");
            foreach (var p in this.PlatformDependencies)
                builder.AppendLine($"{p.DependencyPath} {p.DependencyNameWithoutExtension} {p.SemVer}");

            builder.AppendLine($"");
            builder.AppendLine($"PluginDependencies");
            foreach (var p in this.PluginDependencies)
                builder.AppendLine($"{p.DependencyPath} {p.DependencyNameWithoutExtension} {p.SemVer}");

            builder.AppendLine($"");
            builder.AppendLine($"PluginResourceDependencies");
            foreach (var p in this.PluginResourceDependencies)
                builder.AppendLine($"{p.Path}");

            return builder.ToString();
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                this.FullPathToPluginAssembly = null;
                this.HostDependencies = null;
                this.RemoteDependencies = null;
                this.PluginDependencies = null;
                this.PluginResourceDependencies = null;
                this.PlatformDependencies = null;
                this.AdditionalProbingPaths = null;
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}