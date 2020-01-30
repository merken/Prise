using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyModel;
using Prise.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace Prise
{
    public class PluginDependencyContext : IPluginDependencyContext
    {
        private bool disposed = false;

        private PluginDependencyContext(
            IPluginLoadContext pluginLoadContext,
            IEnumerable<HostDependency> hostDependencies,
            IEnumerable<RemoteDependency> remoteDependencies,
            IEnumerable<PluginDependency> pluginDependencies,
            IEnumerable<PluginDependency> pluginReferenceDependencies,
            IEnumerable<PluginResourceDependency> pluginResourceDependencies,
            IEnumerable<PlatformDependency> platformDependencies
            )
        {
            this.PluginLoadContext = pluginLoadContext;
            this.HostDependencies = hostDependencies;
            this.RemoteDependencies = remoteDependencies;
            this.PluginDependencies = pluginDependencies;
            this.PluginReferenceDependencies = pluginReferenceDependencies;
            this.PluginResourceDependencies = pluginResourceDependencies;
            this.PlatformDependencies = platformDependencies;
        }

        public static PluginDependencyContext FromPluginAssembly<T>(
            IPluginLoadContext pluginLoadContext,
            IPluginLogger<T> pluginLogger,
            IHostFrameworkProvider hostFrameworkProvider,
            IEnumerable<Type> hostTypes,
            IEnumerable<Type> remoteTypes,
            IRuntimePlatformContext runtimePlatformContext,
            IDepsFileProvider<T> depsFileProvider,
            bool ignorePlatformInconsistencies)
        {
            var hostDependencies = new List<HostDependency>();
            var remoteDependencies = new List<RemoteDependency>();

            foreach (var type in hostTypes)
                // Load host types from current app domain
                LoadAssemblyAndReferencesFromCurrentAppDomain(pluginLogger, type.Assembly.GetName(), hostDependencies);

            foreach (var type in remoteTypes)
                remoteDependencies.Add(new RemoteDependency
                {
                    DependencyName = type.Assembly.GetName()
                });

            var hostFramework = hostFrameworkProvider.ProvideHostFramwork();
            var dependencyContext = GetDependencyContextFromPluginAssembly(pluginLoadContext, depsFileProvider);
            var pluginFramework = dependencyContext.Target.Framework;
            CheckFrameworkCompatibility(hostFramework, pluginFramework, ignorePlatformInconsistencies);

            var pluginDependencies = GetPluginDependencies(dependencyContext);
            var pluginReferenceDependencies = GetPluginReferenceDependencies(dependencyContext);
            var resoureDependencies = GetResourceDependencies(dependencyContext);
            var platformDependencies = GetPlatformDependencies(dependencyContext, runtimePlatformContext.GetPlatformExtensions());

            return new PluginDependencyContext(
                pluginLoadContext,
                hostDependencies,
                remoteDependencies,
                pluginDependencies,
                pluginReferenceDependencies,
                resoureDependencies,
                platformDependencies);
        }

        public static async Task<PluginDependencyContext> FromPluginAssemblyAsync<T>(
            IPluginLoadContext pluginLoadContext,
            IPluginLogger<T> pluginLogger,
            IHostFrameworkProvider hostFrameworkProvider,
            IEnumerable<Type> hostTypes,
            IEnumerable<Type> remoteTypes,
            IRuntimePlatformContext runtimePlatformContext,
            IDepsFileProvider<T> depsFileProvider,
            bool ignorePlatformInconsistencies)
        {
            var hostDependencies = new List<HostDependency>();
            var remoteDependencies = new List<RemoteDependency>();

            foreach (var type in hostTypes)
                // Load host types from current app domain
                LoadAssemblyAndReferencesFromCurrentAppDomain(pluginLogger, type.Assembly.GetName(), hostDependencies);

            foreach (var type in remoteTypes)
                remoteDependencies.Add(new RemoteDependency
                {
                    DependencyName = type.Assembly.GetName()
                });

            var hostFramework = hostFrameworkProvider.ProvideHostFramwork();
            var dependencyContext = await GetDependencyContextFromPluginAssemblyAsync(pluginLoadContext, depsFileProvider);
            var pluginFramework = dependencyContext.Target.Framework;
            CheckFrameworkCompatibility(hostFramework, pluginFramework, ignorePlatformInconsistencies);

            var pluginDependencies = GetPluginDependencies(dependencyContext);
            var resoureDependencies = GetResourceDependencies(dependencyContext);
            var platformDependencies = GetPlatformDependencies(dependencyContext, runtimePlatformContext.GetPlatformExtensions());
            var pluginReferenceDependencies = GetPluginReferenceDependencies(dependencyContext);

            return new PluginDependencyContext(
                pluginLoadContext,
                hostDependencies,
                remoteDependencies,
                pluginDependencies,
                pluginReferenceDependencies,
                resoureDependencies,
                platformDependencies);
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
                    throw new PrisePluginException($"Plugin framework {pluginFramework} might have compatibility issues, use the IgnorePlatformInconsistencies flag to skip this check.");

                if (pluginFrameworkType != hostFrameworkType)
                    throw new PrisePluginException($"Plugin framework {pluginFramework} does not match the host {hostFramework}. Please target {hostFramework} in order to load the plugin.");

                var pluginFrameworkVersion = pluginFramework.Split(new String[] { ",Version=v" }, StringSplitOptions.RemoveEmptyEntries)[1];
                var hostFrameworkVersion = hostFramework.Split(new String[] { ",Version=v" }, StringSplitOptions.RemoveEmptyEntries)[1];
                var pluginFrameworkVersionMajor = int.Parse(pluginFrameworkVersion.Split(new String[] { "." }, StringSplitOptions.RemoveEmptyEntries)[0]);
                var pluginFrameworkVersionMinor = int.Parse(pluginFrameworkVersion.Split(new String[] { "." }, StringSplitOptions.RemoveEmptyEntries)[1]);
                var hostFrameworkVersionMajor = int.Parse(hostFrameworkVersion.Split(new String[] { "." }, StringSplitOptions.RemoveEmptyEntries)[0]);
                var hostFrameworkVersionMinor = int.Parse(hostFrameworkVersion.Split(new String[] { "." }, StringSplitOptions.RemoveEmptyEntries)[1]);

                if (pluginFrameworkVersionMajor > hostFrameworkVersionMajor || // If the major version of the plugin is higher
                    (pluginFrameworkVersionMajor == hostFrameworkVersionMajor && pluginFrameworkVersionMinor > hostFrameworkVersionMinor)) // Or the major version is the same but the minor version is higher
                    throw new PrisePluginException($"Plugin framework version {pluginFramework} is newer than the host {hostFramework}. Please upgrade the host to load this plugin.");
            }
        }

        private static void LoadAssemblyAndReferencesFromCurrentAppDomain(IPluginLogger logger, AssemblyName assemblyName, List<HostDependency> hostDependencies)
        {
            if (assemblyName?.Name == null || hostDependencies.Any(h => h.DependencyName.Name == assemblyName.Name))
                return; // Break condition

            hostDependencies.Add(new HostDependency
            {
                DependencyName = assemblyName
            });

            try
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
                foreach (var reference in assembly.GetReferencedAssemblies())
                    LoadAssemblyAndReferencesFromCurrentAppDomain(logger, reference, hostDependencies);
            }
            catch (FileNotFoundException)
            {
                // This happens when the assembly is a platform assembly, log it
                logger.LoadReferenceFromAppDomainFailed(assemblyName);
            }
        }

        private static IEnumerable<PluginDependency> GetPluginDependencies(DependencyContext pluginDependencyContext)
        {
            var dependencies = new List<PluginDependency>();
            var runtimeId = RuntimeEnvironment.GetRuntimeIdentifier();
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
                        Version = runtimeLibrary.Version,
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
                        Version = referenceAssembly.Version,
                        DependencyPath = Path.Join("refs", assembly)
                    });
                }
            }
            return dependencies;
        }

        private static IEnumerable<PlatformDependency> GetPlatformDependencies(DependencyContext pluginDependencyContext, IEnumerable<string> platformExtensions)
        {
            var dependencies = new List<PlatformDependency>();
            var runtimeId = RuntimeEnvironment.GetRuntimeIdentifier();
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

        private static DependencyContext GetDependencyContextFromPluginAssembly<T>(IPluginLoadContext pluginLoadContext, IDepsFileProvider<T> depsFileProvider)
        {
            return new DependencyContextJsonReader().Read(depsFileProvider.ProvideDepsFile(pluginLoadContext).Result);
        }

        private static async Task<DependencyContext> GetDependencyContextFromPluginAssemblyAsync<T>(IPluginLoadContext pluginLoadContext, IDepsFileProvider<T> depsFileProvider)
        {
            return new DependencyContextJsonReader().Read(await depsFileProvider.ProvideDepsFile(pluginLoadContext));
        }

        public IPluginLoadContext PluginLoadContext { get; private set; }
        public IEnumerable<HostDependency> HostDependencies { get; private set; }
        public IEnumerable<RemoteDependency> RemoteDependencies { get; private set; }
        public IEnumerable<PluginDependency> PluginDependencies { get; private set; }
        public IEnumerable<PluginDependency> PluginReferenceDependencies { get; private set; }
        public IEnumerable<PluginResourceDependency> PluginResourceDependencies { get; private set; }
        public IEnumerable<PlatformDependency> PlatformDependencies { get; private set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                this.PluginLoadContext = null;
                this.HostDependencies = null;
                this.RemoteDependencies = null;
                this.PluginDependencies = null;
                this.PluginResourceDependencies = null;
                this.PlatformDependencies = null;
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
