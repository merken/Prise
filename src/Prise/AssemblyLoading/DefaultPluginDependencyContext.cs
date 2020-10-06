using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;
using Prise.Core;
using Prise.Platform;
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
        private DefaultPluginDependencyContext(string fullPathToPluginAssembly,
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
                builder.AppendLine($"{p.DependencyPath} {p.DependencyNameWithoutExtension} {p.Version}");

            builder.AppendLine($"");
            builder.AppendLine($"PluginDependencies");
            foreach (var p in this.PluginDependencies)
                builder.AppendLine($"{p.DependencyPath} {p.DependencyNameWithoutExtension} {p.Version}");

            builder.AppendLine($"");
            builder.AppendLine($"PluginResourceDependencies");
            foreach (var p in this.PluginResourceDependencies)
                builder.AppendLine($"{p.Path}");

            return builder.ToString();
        }

        public static Task<IPluginDependencyContext> FromPluginLoadContext(IPluginLoadContext pluginLoadContext)
        {
            var hostDependencies = new List<HostDependency>();
            var remoteDependencies = new List<RemoteDependency>();
            var runtimePlatformContext = pluginLoadContext.RuntimePlatformContext.ThrowIfNull(nameof(pluginLoadContext.RuntimePlatformContext));

            foreach (var type in pluginLoadContext.HostTypes)
                // Load host types from current app domain
                LoadAssemblyAndReferencesFromCurrentAppDomain(type.Assembly.GetName(), hostDependencies, pluginLoadContext.DowngradableHostTypes, pluginLoadContext.DowngradableHostAssemblies);

            foreach (var assemblyFileName in pluginLoadContext.HostAssemblies)
                // Load host types from current app domain
                LoadAssemblyAndReferencesFromCurrentAppDomain(assemblyFileName, hostDependencies, pluginLoadContext.DowngradableHostTypes, pluginLoadContext.DowngradableHostAssemblies);

            foreach (var type in pluginLoadContext.RemoteTypes)
                remoteDependencies.Add(new RemoteDependency
                {
                    DependencyName = type.Assembly.GetName()
                });

            var dependencyContext = GetDependencyContext(pluginLoadContext.FullPathToPluginAssembly);
            var pluginFramework = dependencyContext.Target.Framework;
            CheckFrameworkCompatibility(pluginLoadContext.HostFramework, pluginFramework, pluginLoadContext.IgnorePlatformInconsistencies);

            var pluginDependencies = GetPluginDependencies(dependencyContext);
            var resourceDependencies = GetResourceDependencies(dependencyContext);
            var platformDependencies = GetPlatformDependencies(dependencyContext, runtimePlatformContext.GetPlatformExtensions());
            // var pluginReferenceDependencies = GetPluginReferenceDependencies(dependencyContext);

            var pluginDependencyContext = new DefaultPluginDependencyContext(
                pluginLoadContext.FullPathToPluginAssembly,
                hostDependencies,
                remoteDependencies,
                pluginDependencies,
                resourceDependencies,
                platformDependencies,
                pluginLoadContext.AdditionalProbingPaths
            );
            Validate(pluginDependencyContext);
            return Task.FromResult<IPluginDependencyContext>(pluginDependencyContext);
        }

        private static void Validate(DefaultPluginDependencyContext dependencyContext)
        {
            var hostDependenciesThatExistInPlugin = dependencyContext.HostDependencies
                                                    .Join(dependencyContext.PluginDependencies, h => h.DependencyName.Name, p => p.DependencyNameWithoutExtension, (h, p) => new
                                                    {
                                                        Host = h,
                                                        Plugin = p
                                                    });

            foreach (var duplicateDependency in hostDependenciesThatExistInPlugin)
            {
                Debug.WriteLine($"Plugin dependency {duplicateDependency.Plugin.DependencyNameWithoutExtension} {duplicateDependency.Plugin.Version} exists in the host");
                if (duplicateDependency.Host.DependencyName.Version > duplicateDependency.Plugin.Version)
                    Debug.WriteLine($"Host dependency {duplicateDependency.Host.DependencyName.Name} version {duplicateDependency.Host.DependencyName.Version} is newer than the plugin {duplicateDependency.Plugin.Version}");
            }
        }

        private static void CheckFrameworkCompatibility(string hostFramework, string pluginFramework, bool ignorePlatformInconsistencies)
        {
            if (ignorePlatformInconsistencies)
                return;

            if (pluginFramework != hostFramework)
            {
                Debug.WriteLine($"Plugin framework {pluginFramework} does not match host framework {hostFramework}");

                var pluginFrameworkType = pluginFramework.Split(new String[] { ",Version=v" }, StringSplitOptions.RemoveEmptyEntries)[0];
                var hostFrameworkType = hostFramework.Split(new String[] { ",Version=v" }, StringSplitOptions.RemoveEmptyEntries)[0];
                if (pluginFrameworkType.ToLower() == ".netstandard")
                    throw new AssemblyLoadingException($"Plugin framework {pluginFramework} might have compatibility issues with the host {hostFramework}, use the IgnorePlatformInconsistencies flag to skip this check.");

                if (pluginFrameworkType != hostFrameworkType)
                    throw new AssemblyLoadingException($"Plugin framework {pluginFramework} does not match the host {hostFramework}. Please target {hostFramework} in order to load the plugin.");

                var pluginFrameworkVersion = pluginFramework.Split(new String[] { ",Version=v" }, StringSplitOptions.RemoveEmptyEntries)[1];
                var hostFrameworkVersion = hostFramework.Split(new String[] { ",Version=v" }, StringSplitOptions.RemoveEmptyEntries)[1];
                var pluginFrameworkVersionMajor = int.Parse(pluginFrameworkVersion.Split(new String[] { "." }, StringSplitOptions.RemoveEmptyEntries)[0]);
                var pluginFrameworkVersionMinor = int.Parse(pluginFrameworkVersion.Split(new String[] { "." }, StringSplitOptions.RemoveEmptyEntries)[1]);
                var hostFrameworkVersionMajor = int.Parse(hostFrameworkVersion.Split(new String[] { "." }, StringSplitOptions.RemoveEmptyEntries)[0]);
                var hostFrameworkVersionMinor = int.Parse(hostFrameworkVersion.Split(new String[] { "." }, StringSplitOptions.RemoveEmptyEntries)[1]);

                if (pluginFrameworkVersionMajor > hostFrameworkVersionMajor || // If the major version of the plugin is higher
                    (pluginFrameworkVersionMajor == hostFrameworkVersionMajor && pluginFrameworkVersionMinor > hostFrameworkVersionMinor)) // Or the major version is the same but the minor version is higher
                    throw new AssemblyLoadingException($"Plugin framework version {pluginFramework} is newer than the host {hostFramework}. Please upgrade the host to load this plugin.");
            }
        }

        private static void LoadAssemblyAndReferencesFromCurrentAppDomain(AssemblyName assemblyName, List<HostDependency> hostDependencies, IEnumerable<Type> downgradableHostTypes, IEnumerable<string> downgradableAssemblies)
        {
            if (assemblyName?.Name == null || hostDependencies.Any(h => h.DependencyName.Name == assemblyName.Name))
                return; // Break condition

            hostDependencies.Add(new HostDependency
            {
                DependencyName = assemblyName,
                AllowDowngrade =
                                downgradableHostTypes.Any(t => t.Assembly.GetName().Name == assemblyName.Name) ||
                                downgradableAssemblies.Any(a => a == assemblyName.Name)
            });

            try
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
                foreach (var reference in assembly.GetReferencedAssemblies())
                    LoadAssemblyAndReferencesFromCurrentAppDomain(reference, hostDependencies, downgradableHostTypes, downgradableAssemblies);
            }
            catch (FileNotFoundException)
            {
                // This happens when the assembly is a platform assembly, log it
                // logger.LoadReferenceFromAppDomainFailed(assemblyName);
            }
        }

        private static void LoadAssemblyAndReferencesFromCurrentAppDomain(string assemblyFileName, List<HostDependency> hostDependencies, IEnumerable<Type> downgradableHostTypes, IEnumerable<string> downgradableAssemblies)
        {
            var assemblyName = new AssemblyName(assemblyFileName);
            if (assemblyFileName == null || hostDependencies.Any(h => h.DependencyName.Name == assemblyName.Name))
                return; // Break condition

            hostDependencies.Add(new HostDependency
            {
                DependencyName = assemblyName,
                AllowDowngrade =
                                downgradableHostTypes.Any(t => t.Assembly.GetName().Name == assemblyName.Name) ||
                                downgradableAssemblies.Any(a => a == assemblyName.Name)
            });

            try
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
                foreach (var reference in assembly.GetReferencedAssemblies())
                    LoadAssemblyAndReferencesFromCurrentAppDomain(reference, hostDependencies, downgradableHostTypes, downgradableAssemblies);
            }
            catch (FileNotFoundException)
            {
                // This happens when the assembly is a platform assembly, log it
                // logger.LoadReferenceFromAppDomainFailed(assemblyName);
            }
        }

        private static string GetCorrectRuntimeIdentifier()
        {
            var platform = Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment.OperatingSystemPlatform;
            var runtimeIdentifier = Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment.GetRuntimeIdentifier();
            switch (platform)
            {
                case Microsoft.DotNet.PlatformAbstractions.Platform.Darwin: // OSX
                case Microsoft.DotNet.PlatformAbstractions.Platform.Windows: // Win
                    return runtimeIdentifier;
                // Linux
                case Microsoft.DotNet.PlatformAbstractions.Platform.Unknown:
                case Microsoft.DotNet.PlatformAbstractions.Platform.FreeBSD:
                case Microsoft.DotNet.PlatformAbstractions.Platform.Linux:
                    return $"{Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment.OperatingSystemPlatform.ToString().ToLower()}-{Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment.RuntimeArchitecture}";
            }

            throw new PlatformNotSupportedException($"Prise: RuntimeIdentifier could not be parsed for {runtimeIdentifier}");
        }

        private static IEnumerable<PluginDependency> GetPluginDependencies(DependencyContext pluginDependencyContext)
        {
            var dependencies = new List<PluginDependency>();
            var runtimeId = GetCorrectRuntimeIdentifier();
            var dependencyGraph = DependencyContext.Default.RuntimeGraph.FirstOrDefault(g => g.Runtime == runtimeId);
            // List of supported runtimes, includes the default runtime and the fallbacks for this dependency context
            var runtimes = new List<string> { dependencyGraph?.Runtime }.AddRangeToList<string>(dependencyGraph?.Fallbacks);
            foreach (var runtimeLibrary in pluginDependencyContext.RuntimeLibraries)
            {
                var assets = runtimeLibrary.RuntimeAssemblyGroups.GetDefaultAssets();

                foreach (var runtime in runtimes)
                {
                    var runtimeSpecificGroup = runtimeLibrary.RuntimeAssemblyGroups.FirstOrDefault(g => g.Runtime == runtime);
                    if (runtimeSpecificGroup != null)
                    {
                        assets = runtimeSpecificGroup.AssetPaths;
                        break;
                    }
                }
                foreach (var asset in assets)
                {
                    var path = asset.StartsWith("lib/")
                            ? Path.GetFileName(asset)
                            : asset;

                    dependencies.Add(new PluginDependency
                    {
                        DependencyNameWithoutExtension = Path.GetFileNameWithoutExtension(asset),
                        Version = new Version(runtimeLibrary.Version),
                        DependencyPath = path,
                        ProbingPath = Path.Combine(runtimeLibrary.Name.ToLowerInvariant(), runtimeLibrary.Version, path)
                    });
                }
            }
            return dependencies;
        }


        private static IEnumerable<PluginDependency> GetPluginReferenceDependencies(DependencyContext pluginDependencyContext)
        {
            var dependencies = new List<PluginDependency>();
            foreach (var referenceAssembly in pluginDependencyContext.CompileLibraries.Where(r => r.Type == "referenceassembly"))
            {
                foreach (var assembly in referenceAssembly.Assemblies)
                {
                    dependencies.Add(new PluginDependency
                    {
                        DependencyNameWithoutExtension = Path.GetFileNameWithoutExtension(assembly),
                        Version = new Version(referenceAssembly.Version),
                        DependencyPath = Path.Join("refs", assembly)
                    });
                }
            }
            return dependencies;
        }

        private static IEnumerable<PlatformDependency> GetPlatformDependencies(DependencyContext pluginDependencyContext, IEnumerable<string> platformExtensions)
        {
            var dependencies = new List<PlatformDependency>();
            var runtimeId = GetCorrectRuntimeIdentifier();
            var dependencyGraph = DependencyContext.Default.RuntimeGraph.FirstOrDefault(g => g.Runtime == runtimeId);
            // List of supported runtimes, includes the default runtime and the fallbacks for this dependency context
            var runtimes = new List<string> { dependencyGraph?.Runtime }.AddRangeToList<string>(dependencyGraph?.Fallbacks);
            foreach (var runtimeLibrary in pluginDependencyContext.RuntimeLibraries)
            {
                var assets = runtimeLibrary.NativeLibraryGroups.GetDefaultAssets();

                foreach (var runtime in runtimes)
                {
                    var runtimeSpecificGroup = runtimeLibrary.NativeLibraryGroups.FirstOrDefault(g => g.Runtime == runtime);
                    if (runtimeSpecificGroup != null)
                    {
                        assets = runtimeSpecificGroup.AssetPaths;
                        break;
                    }
                }
                foreach (var asset in assets.Where(a => platformExtensions.Contains(Path.GetExtension(a)))) // Only load assemblies and not debug files
                {
                    dependencies.Add(new PlatformDependency
                    {
                        DependencyNameWithoutExtension = Path.GetFileNameWithoutExtension(asset),
                        Version = runtimeLibrary.Version,
                        DependencyPath = asset
                    });
                }
            }
            return dependencies;
        }

        private static IEnumerable<PluginResourceDependency> GetResourceDependencies(DependencyContext pluginDependencyContext)
        {
            var dependencies = new List<PluginResourceDependency>();
            foreach (var runtimeLibrary in pluginDependencyContext.RuntimeLibraries
                .Where(l => l.ResourceAssemblies != null && l.ResourceAssemblies.Any()))
            {
                dependencies.AddRange(runtimeLibrary.ResourceAssemblies
                    .Where(r => !String.IsNullOrEmpty(Path.GetDirectoryName(Path.GetDirectoryName(r.Path))))
                    .Select(r =>
                        new PluginResourceDependency
                        {
                            Path = Path.Combine(runtimeLibrary.Name.ToLowerInvariant(),
                                runtimeLibrary.Version,
                                r.Path)
                        }));
            }
            return dependencies;
        }

        private static DependencyContext GetDependencyContext(string fullPathToPluginAssembly)
        {
            var file = File.OpenRead(Path.Combine(Path.GetDirectoryName(fullPathToPluginAssembly), $"{Path.GetFileNameWithoutExtension(fullPathToPluginAssembly)}.deps.json"));
            return new DependencyContextJsonReader().Read(file);
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