using Microsoft.Extensions.DependencyModel;
using NuGet.Versioning;
using Prise.Platform;
using Prise.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace Prise.AssemblyLoading
{
    public class DefaultPluginDependencyContextProvider : IPluginDependencyContextProvider
    {
        private readonly IPlatformAbstraction platformAbstraction;
        private readonly IRuntimePlatformContext runtimePlatformContext;
        public DefaultPluginDependencyContextProvider(Func<IPlatformAbstraction> platformAbstractionFactory, Func<IRuntimePlatformContext> runtimePlatformContextFactory)
        {
            this.platformAbstraction = platformAbstractionFactory.ThrowIfNull(nameof(platformAbstractionFactory))();
            this.runtimePlatformContext = runtimePlatformContextFactory.ThrowIfNull(nameof(runtimePlatformContextFactory))();
        }

        public Task<IPluginDependencyContext> FromPluginLoadContext(IPluginLoadContext pluginLoadContext)
        {
            var hostDependencies = new List<HostDependency>();
            var remoteDependencies = new List<RemoteDependency>();
            // var runtimePlatformContext = pluginLoadContext.RuntimePlatformContext.ThrowIfNull(nameof(pluginLoadContext.RuntimePlatformContext));

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
            var platformDependencies = GetPlatformDependencies(dependencyContext, this.runtimePlatformContext.GetPlatformExtensions());

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
                Debug.WriteLine($"Plugin dependency {duplicateDependency.Plugin.DependencyNameWithoutExtension} {duplicateDependency.Plugin.SemVer} exists in the host");
                if (duplicateDependency.Host.SemVer > duplicateDependency.Plugin.SemVer)
                    Debug.WriteLine($"Host dependency {duplicateDependency.Host.DependencyName.Name} version {duplicateDependency.Host.SemVer} is newer than the Plugin {duplicateDependency.Plugin.SemVer}");
                if (duplicateDependency.Host.SemVer < duplicateDependency.Plugin.SemVer)
                    Debug.WriteLine($"Plugin dependency {duplicateDependency.Plugin.DependencyNameWithoutExtension} version {duplicateDependency.Plugin.SemVer} is newer than the Host {duplicateDependency.Host.SemVer}");
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
                    throw new AssemblyLoadingException($"Plugin framework version {pluginFramework} is newer than the Host {hostFramework}. Please upgrade the Host to load this Plugin.");
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

#if SUPPORTS_NATIVE_PLATFORM_ABSTRACTIONS
        private string GetCorrectRuntimeIdentifier()
        {

            var runtimeIdentifier = RuntimeInformation.RuntimeIdentifier;

            if (this.platformAbstraction.IsOSX() || this.platformAbstraction.IsWindows())
                return runtimeIdentifier;

            // Other: Linux, FreeBSD, ...
            return $"linux-{RuntimeInformation.ProcessArchitecture.ToString().ToLower()}";
        }
#else
        private string GetCorrectRuntimeIdentifier()
        {

            var runtimeIdentifier = Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment.GetRuntimeIdentifier();
            if (this.platformAbstraction.IsOSX() || this.platformAbstraction.IsWindows())
                return runtimeIdentifier;

            return $"{Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment.OperatingSystemPlatform.ToString().ToLower()}-{Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment.RuntimeArchitecture}";
        }
#endif

        private IEnumerable<PluginDependency> GetPluginDependencies(DependencyContext pluginDependencyContext)
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
                        SemVer = ParseSemVer(runtimeLibrary.Version),
                        DependencyPath = path,
                        ProbingPath = Path.Combine(runtimeLibrary.Name.ToLowerInvariant(), runtimeLibrary.Version, path)
                    });
                }
            }
            return dependencies;
        }

        private SemanticVersion ParseSemVer(string version)
        {
            if (SemanticVersion.TryParse(version, out var semVer))
                return semVer;

            var versions = version.Split('.');
            if (versions.Length > 3)
                return new SemanticVersion(int.Parse(versions[0]), int.Parse(versions[1]), int.Parse(versions[2]), versions[3]);

            return new SemanticVersion(int.Parse(versions[0]), int.Parse(versions[1]), int.Parse(versions[2]));
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
                        SemVer = SemanticVersion.Parse(referenceAssembly.Version),
                        DependencyPath = Path.Join("refs", assembly)
                    });
                }
            }
            return dependencies;
        }

        private IEnumerable<PlatformDependency> GetPlatformDependencies(DependencyContext pluginDependencyContext, IEnumerable<string> platformExtensions)
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
                    SemanticVersion semVer;
                    if (!SemanticVersion.TryParse(runtimeLibrary.Version, out semVer))
                        // Take first 3 digits
                        semVer = SemanticVersion.Parse(String.Join('.',runtimeLibrary.Version.Split(".").Take(3).ToArray()));
                   
                    dependencies.Add(new PlatformDependency
                    {
                        DependencyNameWithoutExtension = Path.GetFileNameWithoutExtension(asset),
                        SemVer = semVer,
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
    }
}