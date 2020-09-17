using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;

namespace Prise.V2
{

    /// <summary>
    /// Sets the preferred Native Library loading strategy
    /// </summary>
    public enum NativeDependencyLoadPreference
    {
        // Native libraries will be loaded from the runtime folder on the host
        // Windows: C:\Program Files\dotnet\shared\..
        // Linux: /usr/share/dotnet/shared/..
        // OSX: /usr/local/share/dotnet/shared
        PreferInstalledRuntime = 0,
        // Native libraries will be loaded from the remote context 
        PreferDependencyContext
    }

    public enum RuntimeType
    {
        UnSpecified = 0,
        AspNetCoreAll,
        AspNetCoreApp,
        NetCoreApp,
        WindowsDesktopApp,
    }

    public class Runtime
    {
        public RuntimeType RuntimeType { get; set; }
        public string Version { get; set; }
        public string Location { get; set; }
    }

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

    public interface IRuntimePlatformContext
    {
        IEnumerable<string> GetPlatformExtensions();
        IEnumerable<string> GetPluginDependencyNames(string nameWithoutFileExtension);
        IEnumerable<string> GetPlatformDependencyNames(string nameWithoutFileExtension);
        RuntimeInfo GetRuntimeInfo();
    }

    public class RuntimeInfo
    {
        public IEnumerable<Runtime> Runtimes { get; set; }
    }

    public class DefaultRuntimePlatformContext : IRuntimePlatformContext
    {
        public IEnumerable<string> GetPlatformExtensions() => GetPlatformDependencyFileExtensions();

        public IEnumerable<string> GetPluginDependencyNames(string nameWithoutFileExtension) =>
            GetPluginDependencyFileExtensions()
                .Select(ext => $"{nameWithoutFileExtension}{ext}");

        public IEnumerable<string> GetPlatformDependencyNames(string nameWithoutFileExtension) =>
             GetPlatformDependencyFileCandidates(nameWithoutFileExtension);

        public RuntimeInfo GetRuntimeInfo()
        {
            var runtimeBasePath = String.Empty;
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                runtimeBasePath = "C:\\Program Files\\dotnet\\shared";
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                runtimeBasePath = "/usr/share/dotnet/shared";
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                runtimeBasePath = "/usr/local/share/dotnet/shared";

            var platformIndependendPath = System.IO.Path.GetFullPath(runtimeBasePath);
            var runtimes = new List<Runtime>();
            foreach (var pathToDirectory in System.IO.Directory.GetDirectories(platformIndependendPath))
            {
                var runtimeName = System.IO.Path.GetFileName(pathToDirectory); // Gets the directory name
                var runtimeType = ParseType(runtimeName);
                foreach (var pathToVersion in System.IO.Directory.GetDirectories(pathToDirectory))
                {
                    var runtimeVersion = System.IO.Path.GetFileName(pathToVersion); // Gets the directory name
                    var runtimeLocation = pathToVersion;
                    runtimes.Add(new Runtime
                    {
                        Version = runtimeVersion,
                        Location = runtimeLocation,
                        RuntimeType = runtimeType
                    });
                }
            }

            return new RuntimeInfo
            {
                Runtimes = runtimes
            };
        }

        private RuntimeType ParseType(string runtimeName)
        {
            switch (runtimeName.ToUpper())
            {
                case "MICROSOFT.ASPNETCORE.ALL": return RuntimeType.AspNetCoreAll;
                case "MICROSOFT.ASPNETCORE.APP": return RuntimeType.AspNetCoreApp;
                case "MICROSOFT.NETCORE.APP": return RuntimeType.NetCoreApp;
                case "MICROSOFT.WINDOWSDESKTOP.APP": return RuntimeType.WindowsDesktopApp;
            }
            throw new AssemblyLoadException($"Runtime {runtimeName} could not be parsed");
        }

        private string[] GetPluginDependencyFileExtensions()
        {
            return new[]
            {
                ".dll",
                ".ni.dll",
                ".exe",
                ".ni.exe"
            };
        }

        private string[] GetPlatformDependencyFileCandidates(string fileNameWithoutExtension)
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new[] { $"{fileNameWithoutExtension}.dll" };
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return new[] {
                    $"{fileNameWithoutExtension}.so",
                    $"{fileNameWithoutExtension}.so.1",
                    $"lib{fileNameWithoutExtension}.so",
                    $"lib{fileNameWithoutExtension}.so.1" };
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return new[] {
                    $"{fileNameWithoutExtension}.dylib",
                    $"lib{fileNameWithoutExtension}.dylib" };

            throw new AssemblyLoadException($"Platform {System.Runtime.InteropServices.RuntimeInformation.OSDescription} is not supported");
        }

        private string[] GetPlatformDependencyFileExtensions()
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new[] { ".dll" };
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return new[] { ".so", ".so.1" };
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return new[] { ".dylib" };

            throw new AssemblyLoadException($"Platform {System.Runtime.InteropServices.RuntimeInformation.OSDescription} is not supported");
        }
    }

    public class DefaultPluginDependencyResolver : IPluginDependencyResolver
    {
        protected readonly IRuntimePlatformContext runtimePlatformContext;
        protected bool disposed = false;

        public DefaultPluginDependencyResolver(IRuntimePlatformContext runtimePlatformContext)
        {
            this.runtimePlatformContext = runtimePlatformContext;
        }

        public virtual Stream ResolvePluginDependencyToPath(string fullPathToPluginAssembly, PluginDependency dependency)
        {
            var localFile = Path.Combine(Path.GetDirectoryName(fullPathToPluginAssembly), dependency.DependencyPath);
            if (File.Exists(localFile))
            {
                return File.OpenRead(localFile);
            }

            // TODO Add support for additional probing paths

            foreach (var candidate in runtimePlatformContext.GetPluginDependencyNames(dependency.DependencyNameWithoutExtension))
            {
                var candidateLocation = Path.Combine(Path.GetDirectoryName(fullPathToPluginAssembly), candidate);
                if (File.Exists(candidateLocation))
                {
                    return File.OpenRead(candidateLocation);
                }
            }
            return null;
        }

        public virtual string ResolvePlatformDependencyToPath(string fullPathToPluginAssembly, PlatformDependency dependency)
        {
            foreach (var candidate in runtimePlatformContext.GetPlatformDependencyNames(dependency.DependencyNameWithoutExtension))
            {
                var candidateLocation = Path.Combine(Path.GetDirectoryName(fullPathToPluginAssembly), candidate);
                if (File.Exists(candidateLocation))
                {
                    return candidateLocation;
                }
            }

            var local = Path.Combine(Path.GetDirectoryName(fullPathToPluginAssembly), dependency.DependencyPath);
            if (File.Exists(local))
            {
                return local;
            }

            // Todo add support for custom probing paths
            return null;
        }

        public virtual string ResolvePlatformDependencyPathToRuntime(PluginPlatformVersion pluginPlatformVersion, string platformDependencyPath)
        {
            var platformDependencyFileVersion = FileVersionInfo.GetVersionInfo(platformDependencyPath);
            var platformDependencyName = Path.GetFileName(platformDependencyPath);
            var runtimeInformation = this.runtimePlatformContext.GetRuntimeInfo();

            var runtimes = runtimeInformation.Runtimes;
            if (pluginPlatformVersion.IsSpecified)
            {
                // First filter on specific version
                runtimes = runtimes.Where(r => r.Version == pluginPlatformVersion.Version);
                // Then, filter on target runtime, this is not always provided
                if (pluginPlatformVersion.Runtime != RuntimeType.UnSpecified)
                    runtimes = runtimes.Where(r => r.RuntimeType == pluginPlatformVersion.Runtime);

                if (!runtimes.Any())
                    throw new AssemblyLoadException($"Requisted platform was not installed {pluginPlatformVersion.Runtime} {pluginPlatformVersion.Version}");
            }

            foreach (var runtime in runtimes.OrderByDescending(r => r.Version))
            {
                var candidateFilePath = Directory.GetFiles(runtime.Location).FirstOrDefault(f => String.Compare(Path.GetFileName(f), platformDependencyName) == 0);
                if (!String.IsNullOrEmpty(candidateFilePath))
                {
                    var candidateFileVersion = FileVersionInfo.GetVersionInfo(candidateFilePath);
                    if (String.Compare(platformDependencyFileVersion.FileVersion, candidateFileVersion.FileVersion) == 0)
                        return candidateFilePath;
                }
            }

            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                // Nothing to do here
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public interface IPluginDependencyResolver : IDisposable
    {
        Stream ResolvePluginDependencyToPath(string fullPathToPluginAssembly, PluginDependency dependency);
        string ResolvePlatformDependencyToPath(string fullPathToPluginAssembly, PlatformDependency dependency);
        string ResolvePlatformDependencyPathToRuntime(PluginPlatformVersion pluginPlatformVersion, string platformDependencyPath);
    }

    public class HostDependency
    {
        public AssemblyName DependencyName { get; set; }
        public bool AllowDowngrade { get; set; }
    }

    public class RemoteDependency
    {
        public AssemblyName DependencyName { get; set; }
    }

    public class PluginDependency
    {
        public string DependencyNameWithoutExtension { get; set; }
        public string Version { get; set; }
        public string DependencyPath { get; set; }
        public string ProbingPath { get; set; }
    }

    public class PluginResourceDependency
    {
        public string Path { get; set; }
    }

    public class PlatformDependency
    {
        public string DependencyNameWithoutExtension { get; set; }
        public string Version { get; set; }
        public string DependencyPath { get; set; }
        public string ProbingPath { get; set; }
    }

    public static class DependencyUtils
    {
        public static IEnumerable<T> AddRangeToList<T>(this List<T> list, IEnumerable<T> range)
        {
            if (range != null)
                list.AddRange(range);
            return list;
        }
    }

    public class DefaultPluginDependencyContext : IPluginDependencyContext
    {
        private bool disposed = false;

        private DefaultPluginDependencyContext(
            string fullPathToPluginAssembly,
            IEnumerable<HostDependency> hostDependencies,
            IEnumerable<RemoteDependency> remoteDependencies,
            IEnumerable<PluginDependency> pluginDependencies,
            IEnumerable<PluginDependency> pluginReferenceDependencies,
            IEnumerable<PluginResourceDependency> pluginResourceDependencies,
            IEnumerable<PlatformDependency> platformDependencies
            )
        {
            this.FullPathToPluginAssembly = fullPathToPluginAssembly;
            this.HostDependencies = hostDependencies;
            this.RemoteDependencies = remoteDependencies;
            this.PluginDependencies = pluginDependencies;
            this.PluginReferenceDependencies = pluginReferenceDependencies;
            this.PluginResourceDependencies = pluginResourceDependencies;
            this.PlatformDependencies = platformDependencies;
        }

        public static async Task<DefaultPluginDependencyContext> FromPluginLoadContext(IPluginLoadContext pluginLoadContext)
        {
            var hostDependencies = new List<HostDependency>();
            var remoteDependencies = new List<RemoteDependency>();
            var runtimePlatformContext = pluginLoadContext.RuntimePlatformContext;

            foreach (var type in pluginLoadContext.HostTypes)
                // Load host types from current app domain
                LoadAssemblyAndReferencesFromCurrentAppDomain(type.Assembly.GetName(), hostDependencies, pluginLoadContext.DowngradableTypes, pluginLoadContext.DowngradableHostAssemblies);

            foreach (var assemblyFileName in pluginLoadContext.HostAssemblies)
                // Load host types from current app domain
                LoadAssemblyAndReferencesFromCurrentAppDomain(assemblyFileName, hostDependencies, pluginLoadContext.DowngradableTypes, pluginLoadContext.DowngradableHostAssemblies);

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
            var pluginReferenceDependencies = GetPluginReferenceDependencies(dependencyContext);

            return new DefaultPluginDependencyContext(
                pluginLoadContext.FullPathToPluginAssembly,
                hostDependencies,
                remoteDependencies,
                pluginDependencies,
                pluginReferenceDependencies,
                resourceDependencies,
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
                    throw new AssemblyLoadException($"Plugin framework {pluginFramework} might have compatibility issues with the host {hostFramework}, use the IgnorePlatformInconsistencies flag to skip this check.");

                if (pluginFrameworkType != hostFrameworkType)
                    throw new AssemblyLoadException($"Plugin framework {pluginFramework} does not match the host {hostFramework}. Please target {hostFramework} in order to load the plugin.");

                var pluginFrameworkVersion = pluginFramework.Split(new String[] { ",Version=v" }, StringSplitOptions.RemoveEmptyEntries)[1];
                var hostFrameworkVersion = hostFramework.Split(new String[] { ",Version=v" }, StringSplitOptions.RemoveEmptyEntries)[1];
                var pluginFrameworkVersionMajor = int.Parse(pluginFrameworkVersion.Split(new String[] { "." }, StringSplitOptions.RemoveEmptyEntries)[0]);
                var pluginFrameworkVersionMinor = int.Parse(pluginFrameworkVersion.Split(new String[] { "." }, StringSplitOptions.RemoveEmptyEntries)[1]);
                var hostFrameworkVersionMajor = int.Parse(hostFrameworkVersion.Split(new String[] { "." }, StringSplitOptions.RemoveEmptyEntries)[0]);
                var hostFrameworkVersionMinor = int.Parse(hostFrameworkVersion.Split(new String[] { "." }, StringSplitOptions.RemoveEmptyEntries)[1]);

                if (pluginFrameworkVersionMajor > hostFrameworkVersionMajor || // If the major version of the plugin is higher
                    (pluginFrameworkVersionMajor == hostFrameworkVersionMajor && pluginFrameworkVersionMinor > hostFrameworkVersionMinor)) // Or the major version is the same but the minor version is higher
                    throw new AssemblyLoadException($"Plugin framework version {pluginFramework} is newer than the host {hostFramework}. Please upgrade the host to load this plugin.");
            }
        }

        private static void LoadAssemblyAndReferencesFromCurrentAppDomain(AssemblyName assemblyName, List<HostDependency> hostDependencies, IEnumerable<Type> downgradableTypes, IEnumerable<string> downgradableAssemblies)
        {
            if (assemblyName?.Name == null || hostDependencies.Any(h => h.DependencyName.Name == assemblyName.Name))
                return; // Break condition

            hostDependencies.Add(new HostDependency
            {
                DependencyName = assemblyName,
                AllowDowngrade =
                                downgradableTypes.Any(t => t.Assembly.GetName().Name == assemblyName.Name) ||
                                downgradableAssemblies.Any(a => a == assemblyName.Name)
            });

            try
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
                foreach (var reference in assembly.GetReferencedAssemblies())
                    LoadAssemblyAndReferencesFromCurrentAppDomain(reference, hostDependencies, downgradableTypes, downgradableAssemblies);
            }
            catch (FileNotFoundException)
            {
                // This happens when the assembly is a platform assembly, log it
                // logger.LoadReferenceFromAppDomainFailed(assemblyName);
            }
        }

        private static void LoadAssemblyAndReferencesFromCurrentAppDomain(string assemblyFileName, List<HostDependency> hostDependencies, IEnumerable<Type> downgradableTypes, IEnumerable<string> downgradableAssemblies)
        {
            var assemblyName = new AssemblyName(assemblyFileName);
            if (assemblyFileName == null || hostDependencies.Any(h => h.DependencyName.Name == assemblyName.Name))
                return; // Break condition

            hostDependencies.Add(new HostDependency
            {
                DependencyName = assemblyName,
                AllowDowngrade =
                                downgradableTypes.Any(t => t.Assembly.GetName().Name == assemblyName.Name) ||
                                downgradableAssemblies.Any(a => a == assemblyName.Name)
            });

            try
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
                foreach (var reference in assembly.GetReferencedAssemblies())
                    LoadAssemblyAndReferencesFromCurrentAppDomain(reference, hostDependencies, downgradableTypes, downgradableAssemblies);
            }
            catch (FileNotFoundException)
            {
                // This happens when the assembly is a platform assembly, log it
                // logger.LoadReferenceFromAppDomainFailed(assemblyName);
            }
        }

        private static IEnumerable<PluginDependency> GetPluginDependencies(DependencyContext pluginDependencyContext)
        {
            var dependencies = new List<PluginDependency>();
            var runtimeId = Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment.GetRuntimeIdentifier();
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
            var runtimeId = Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment.GetRuntimeIdentifier();
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

        public string FullPathToPluginAssembly { get; private set; }
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
                this.FullPathToPluginAssembly = null;
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

    public interface IPluginDependencyContext : IDisposable
    {
        string FullPathToPluginAssembly { get; }
        IEnumerable<HostDependency> HostDependencies { get; }
        IEnumerable<RemoteDependency> RemoteDependencies { get; }
        IEnumerable<PluginDependency> PluginDependencies { get; }
        IEnumerable<PluginDependency> PluginReferenceDependencies { get; }
        IEnumerable<PluginResourceDependency> PluginResourceDependencies { get; }
        IEnumerable<PlatformDependency> PlatformDependencies { get; }
    }
}