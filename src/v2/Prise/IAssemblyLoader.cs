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

        public static async Task<DefaultPluginDependencyContext> FromPluginAssembly(
            string fullPathToPluginAssembly,
            string hostFramework,
            IEnumerable<Type> hostTypes,
            IEnumerable<string> hostAssemblies,
            IEnumerable<Type> downgradableTypes,
            IEnumerable<string> downgradableHostAssemblies,
            IEnumerable<Type> remoteTypes,
            IRuntimePlatformContext runtimePlatformContext,
            bool ignorePlatformInconsistencies)
        {
            var hostDependencies = new List<HostDependency>();
            var remoteDependencies = new List<RemoteDependency>();

            foreach (var type in hostTypes)
                // Load host types from current app domain
                LoadAssemblyAndReferencesFromCurrentAppDomain(type.Assembly.GetName(), hostDependencies, downgradableTypes, downgradableHostAssemblies);

            foreach (var assemblyFileName in hostAssemblies)
                // Load host types from current app domain
                LoadAssemblyAndReferencesFromCurrentAppDomain(assemblyFileName, hostDependencies, downgradableTypes, downgradableHostAssemblies);

            foreach (var type in remoteTypes)
                remoteDependencies.Add(new RemoteDependency
                {
                    DependencyName = type.Assembly.GetName()
                });

            var dependencyContext = GetDependencyContext(fullPathToPluginAssembly);
            var pluginFramework = dependencyContext.Target.Framework;
            CheckFrameworkCompatibility(hostFramework, pluginFramework, ignorePlatformInconsistencies);

            var pluginDependencies = GetPluginDependencies(dependencyContext);
            var resoureDependencies = GetResourceDependencies(dependencyContext);
            var platformDependencies = GetPlatformDependencies(dependencyContext, runtimePlatformContext.GetPlatformExtensions());
            var pluginReferenceDependencies = GetPluginReferenceDependencies(dependencyContext);

            return new DefaultPluginDependencyContext(
                fullPathToPluginAssembly,
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

    public class ValueOrProceed<T>
    {
        public static ValueOrProceed<T> Proceed() => new ValueOrProceed<T>
        {
            CanProceed = true
        };

        public static ValueOrProceed<T> FromValue(T value, bool proceed) => new ValueOrProceed<T>
        {
            Value = value,
            CanProceed = proceed
        };

        public T Value { get; private set; }
        public bool CanProceed { get; private set; }
    }

    /// <summary>
    /// Represents a native library through either its full Path or a Pointer to an assembly in memory
    /// </summary>
    public class NativeAssembly
    {
        public string Path { get; private set; }
        public IntPtr Pointer { get; private set; }

        public static NativeAssembly Create(string path, IntPtr pointer) => new NativeAssembly { Path = path, Pointer = pointer };
    }

    public class AssemblyFromStrategy
    {
        public Assembly Assembly { get; set; }
        public bool CanBeReleased { get; set; }

        public static AssemblyFromStrategy Releasable(Assembly assembly) => new AssemblyFromStrategy() { Assembly = assembly, CanBeReleased = true };
        public static AssemblyFromStrategy NotReleasable(Assembly assembly) => new AssemblyFromStrategy() { Assembly = assembly, CanBeReleased = false };
    }

    public interface IAssemblyLoadStrategy
    {
        /// <summary>
        /// Loads a dependency assembly for the current plugin
        /// </summary>
        /// <param name="fullPathToPluginAssembly">Full path to the corresponding plugin assembly</param>
        /// <param name="assemblyName"></param>
        /// <param name="loadFromDependencyContext"></param>
        /// <param name="loadFromRemote"></param>
        /// <param name="loadFromAppDomain"></param>
        /// <returns>A loaded assembly</returns>
        AssemblyFromStrategy LoadAssembly(string fullPathToPluginAssembly, AssemblyName assemblyName,
            Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>> loadFromDependencyContext,
            Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>> loadFromRemote,
            Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>> loadFromAppDomain);

        /// <summary>
        /// Loads a native assembly
        /// </summary>
        /// <param name="unmanagedDllName"></param>
        /// <param name="loadFromDependencyContext"></param>
        /// <param name="loadFromRemote"></param>
        /// <param name="loadFromAppDomain"></param>
        /// <returns>The path to a native assembly</returns>
        NativeAssembly LoadUnmanagedDll(string fullPathToPluginAssembly, string unmanagedDllName,
           Func<string, string, ValueOrProceed<string>> loadFromDependencyContext,
           Func<string, string, ValueOrProceed<string>> loadFromRemote,
           Func<string, string, ValueOrProceed<IntPtr>> loadFromAppDomain);
    }

    public interface INativeAssemblyUnloader
    {
        void UnloadNativeAssembly(string fullPathToLoadedNativeAssembly, IntPtr pointerToAssembly);
    }

    public interface IAssemblyLoadContext : IDisposable
    {
        Task<Assembly> LoadPluginAssembly(
            string fullPathToPluginAssembly,
            IEnumerable<Type> hostTypes = null,
            IEnumerable<string> hostAssemblies = null,
            IEnumerable<Type> downgradableTypes = null,
            IEnumerable<string> downgradableHostAssemblies = null,
            IEnumerable<Type> remoteTypes = null,
            bool ignorePlatformInconsistencies = false,
            NativeDependencyLoadPreference nativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferInstalledRuntime,
            PluginPlatformVersion pluginPlatformVersion = null
        );
        Task Unload();
    }

    public interface IPluginAssemblyLoader : IDisposable
    {
        Task<Assembly> Load(string fullPathToAssembly);
        Task Unload(string fullPathToAssembly);
    }

    [Serializable]
    public class AssemblyLoadException : Exception
    {
        public AssemblyLoadException(string message) : base(message)
        {
        }

        public AssemblyLoadException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public AssemblyLoadException()
        {
        }

        protected AssemblyLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class DefaultAssemblyLoadStrategy : IAssemblyLoadStrategy
    {
        protected IPluginDependencyContext pluginDependencyContext;

        public DefaultAssemblyLoadStrategy()
        {
        }

        public virtual AssemblyFromStrategy LoadAssembly(string fullPathToPluginAssembly, AssemblyName assemblyName,
            Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>> loadFromDependencyContext,
            Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>> loadFromRemote,
            Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>> loadFromAppDomain)
        {
            if (assemblyName.Name == null)
                return null;

            ValueOrProceed<AssemblyFromStrategy> valueOrProceed = ValueOrProceed<AssemblyFromStrategy>.FromValue(null, true);

            var isHostAssembly = IsHostAssembly(assemblyName);
            var isRemoteAssembly = IsRemoteAssembly(assemblyName);

            if (isHostAssembly && !isRemoteAssembly) // Load from Default App Domain (host)
            {
                valueOrProceed = loadFromAppDomain(fullPathToPluginAssembly, assemblyName);
                if (valueOrProceed.Value != null)
                    return null; // fallback to default loading mechanism
            }

            if (valueOrProceed.CanProceed)

                valueOrProceed = loadFromDependencyContext(fullPathToPluginAssembly, assemblyName);


            if (valueOrProceed.CanProceed)
                valueOrProceed = loadFromRemote(fullPathToPluginAssembly, assemblyName);

            return valueOrProceed.Value;
        }

        public virtual NativeAssembly LoadUnmanagedDll(string fullPathToPluginAssembly, string unmanagedDllName,
            Func<string, string, ValueOrProceed<string>> loadFromDependencyContext,
            Func<string, string, ValueOrProceed<string>> loadFromRemote,
            Func<string, string, ValueOrProceed<IntPtr>> loadFromAppDomain)
        {
            ValueOrProceed<string> valueOrProceed = ValueOrProceed<string>.FromValue(String.Empty, true);
            ValueOrProceed<IntPtr> ptrValueOrProceed = ValueOrProceed<IntPtr>.FromValue(IntPtr.Zero, true);

            valueOrProceed = loadFromDependencyContext(fullPathToPluginAssembly, unmanagedDllName);

            if (valueOrProceed.CanProceed)
                ptrValueOrProceed = loadFromAppDomain(fullPathToPluginAssembly, unmanagedDllName);

            if (valueOrProceed.CanProceed && ptrValueOrProceed.CanProceed)
                valueOrProceed = loadFromRemote(fullPathToPluginAssembly, unmanagedDllName);

            return NativeAssembly.Create(valueOrProceed.Value, ptrValueOrProceed.Value);
        }

        protected virtual bool IsHostAssembly(AssemblyName assemblyName) => this.pluginDependencyContext.HostDependencies.Any(h => h.DependencyName.Name == assemblyName.Name);
        protected virtual bool IsRemoteAssembly(AssemblyName assemblyName) => this.pluginDependencyContext.RemoteDependencies.Any(r => r.DependencyName.Name == assemblyName.Name);
    }

    public class DefaultNativeAssemblyUnloader : INativeAssemblyUnloader
    {
        public void UnloadNativeAssembly(string fullPathToLoadedNativeAssembly, IntPtr pointerToAssembly)
        {
            NativeLibrary.Free(pointerToAssembly);
        }
    }

    /// <summary>
    /// This base class will load all assemblies in memory by default and is Collectible by default
    /// </summary>
    public abstract class InMemoryAssemblyLoadContext : AssemblyLoadContext
    {
        protected bool isCollectible = false;
        protected InMemoryAssemblyLoadContext() { }

        protected InMemoryAssemblyLoadContext(bool isCollectible) : base($"InMemoryAssemblyLoadContext{Guid.NewGuid().ToString("N")}", isCollectible: isCollectible)
        {
            this.isCollectible = isCollectible;
        }

        public new Assembly LoadFromAssemblyPath(string path)
        {
            try
            {
                using (var stream = File.OpenRead(path))
                    return LoadFromStream(stream);
            }
            catch (InvalidOperationException ex)
            {
                throw new AssemblyLoadException($"Assembly at {path} could not be loaded (locked dll file)", ex);
            }
        }
    }

    public class DefaultAssemblyLoadContext : InMemoryAssemblyLoadContext, IAssemblyLoadContext
    {
        protected AssemblyDependencyResolver resolver;
        protected ConcurrentDictionary<string, IntPtr> loadedNativeLibraries;
        protected bool disposed = false;
        protected bool disposing = false;
        protected ConcurrentBag<string> loadedPlugins;
        protected ConcurrentBag<WeakReference> assemblyReferences;
        protected INativeAssemblyUnloader nativeAssemblyUnloader;
        protected IAssemblyLoadStrategy assemblyLoadStrategy;
        protected IPluginDependencyContext pluginDependencyContext;
        protected IPluginDependencyResolver pluginDependencyResolver;
        protected NativeDependencyLoadPreference nativeDependencyLoadPreference;
        protected string fullPathToPluginAssembly;
        protected string initialPluginLoadDirectory;
        protected PluginPlatformVersion pluginPlatformVersion;

        public DefaultAssemblyLoadContext()
        {
            this.nativeAssemblyUnloader = new DefaultNativeAssemblyUnloader();
            this.assemblyLoadStrategy = new DefaultAssemblyLoadStrategy();
            this.loadedNativeLibraries = new ConcurrentDictionary<string, IntPtr>();
            this.loadedPlugins = new ConcurrentBag<string>();
            this.assemblyReferences = new ConcurrentBag<WeakReference>();
        }

        private void GuardIfAlreadyLoaded(string pluginAssemblyName)
        {
            if (this.disposed || this.disposing)
                throw new AssemblyLoadException($"Cannot load Plugin {pluginAssemblyName} when disposed.");

            if (String.IsNullOrEmpty(pluginAssemblyName))
                throw new AssemblyLoadException($"Cannot load empty Plugin. {nameof(pluginAssemblyName)} was null or empty.");

            if (this.loadedPlugins.Contains(pluginAssemblyName))
                throw new AssemblyLoadException($"Plugin {pluginAssemblyName} was already loaded.");

            this.loadedPlugins.Add(pluginAssemblyName);
        }

        public async Task<Assembly> LoadPluginAssembly(
            string fullPathToPluginAssembly,
            IEnumerable<Type> hostTypes = null,
            IEnumerable<string> hostAssemblies = null,
            IEnumerable<Type> downgradableTypes = null,
            IEnumerable<string> downgradableHostAssemblies = null,
            IEnumerable<Type> remoteTypes = null,
            bool ignorePlatformInconsistencies = false,
            NativeDependencyLoadPreference nativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferInstalledRuntime,
            PluginPlatformVersion pluginPlatformVersion = null
            )
        {
            this.fullPathToPluginAssembly = fullPathToPluginAssembly;
            this.initialPluginLoadDirectory = Path.GetDirectoryName(fullPathToPluginAssembly);
            if (hostTypes == null)
                hostTypes = Enumerable.Empty<Type>();

            if (hostAssemblies == null)
                hostAssemblies = Enumerable.Empty<string>();

            if (downgradableTypes == null)
                downgradableTypes = Enumerable.Empty<Type>();

            if (downgradableHostAssemblies == null)
                downgradableHostAssemblies = Enumerable.Empty<string>();

            if (remoteTypes == null)
                remoteTypes = Enumerable.Empty<Type>();

            try
            {
                this.resolver = new AssemblyDependencyResolver(fullPathToPluginAssembly);
                var hostFramework = Assembly
                    .GetEntryAssembly()?
                    .GetCustomAttribute<TargetFrameworkAttribute>()?
                    .FrameworkName;

                var runtimePlatformContext = new DefaultRuntimePlatformContext();

                this.pluginDependencyContext = await DefaultPluginDependencyContext.FromPluginAssembly(
                    fullPathToPluginAssembly,
                    hostFramework,
                    hostTypes,
                    hostAssemblies,
                    downgradableTypes,
                    downgradableHostAssemblies,
                    remoteTypes,
                    runtimePlatformContext,
                    ignorePlatformInconsistencies
                );
            }
            catch (System.ArgumentException ex)
            {
                throw new AssemblyLoadException($"{nameof(AssemblyDependencyResolver)} could not be instantiated, possible issue with {this.initialPluginLoadDirectory}{Path.GetFileNameWithoutExtension(fullPathToPluginAssembly)}.deps.json file?", ex);
            }

            GuardIfAlreadyLoaded(fullPathToPluginAssembly);

            using (var pluginStream = await LoadFileFromLocalDisk(fullPathToPluginAssembly))
            {
                return LoadAndAddToWeakReferences(pluginStream);
            }
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            // This fixes the issue where the ALC is still alive and utilized in the host
            if (this.disposed || this.disposing)
                return null;

            return LoadAndAddToWeakReferences(assemblyLoadStrategy.LoadAssembly(
                    this.initialPluginLoadDirectory,
                    assemblyName,
                    LoadFromDependencyContext,
                    LoadFromRemote,
                    LoadFromDefaultContext
                ));
        }

        protected virtual bool IsResourceAssembly(AssemblyName assemblyName)
        {
            return !string.IsNullOrEmpty(assemblyName.CultureName) && !string.Equals("neutral", assemblyName.CultureName);
        }

        // <summary>
        /// This override includes the netcore 3.0 resolver
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        protected ValueOrProceed<AssemblyFromStrategy> LoadFromDependencyContext(string fullPathToPluginAssembly, AssemblyName assemblyName)
        {
            var assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
            if (!String.IsNullOrEmpty(assemblyPath) && File.Exists(assemblyPath))
            {
                return ValueOrProceed<AssemblyFromStrategy>.FromValue(AssemblyFromStrategy.Releasable(LoadFromAssemblyPath(assemblyPath)), false);
            }

            if (IsResourceAssembly(assemblyName))
            {
                foreach (var resourceDependency in this.pluginDependencyContext.PluginResourceDependencies)
                {
                    var resourcePath = Path.Combine(resourceDependency.Path, assemblyName.CultureName, assemblyName.Name + ".dll");
                    if (File.Exists(resourcePath))
                    {
                        return ValueOrProceed<AssemblyFromStrategy>.FromValue(AssemblyFromStrategy.Releasable(LoadFromAssemblyPath(resourcePath)), false);
                    }
                }

                // Do not proceed probing
                return ValueOrProceed<AssemblyFromStrategy>.FromValue(null, false);
            }

            var dependencyPath = Path.GetDirectoryName(fullPathToPluginAssembly);

            var pluginDependency = this.pluginDependencyContext.PluginDependencies.FirstOrDefault(d => d.DependencyNameWithoutExtension == assemblyName.Name);
            if (pluginDependency != null)
            {
                var dependency = this.pluginDependencyResolver.ResolvePluginDependencyToPath(dependencyPath, pluginDependency);
                if (dependency != null)
                    return ValueOrProceed<AssemblyFromStrategy>.FromValue(AssemblyFromStrategy.Releasable(LoadFromStream(dependency)), false);
            }

            var localFile = Path.Combine(dependencyPath, assemblyName.Name + ".dll");
            if (File.Exists(localFile))
            {
                return ValueOrProceed<AssemblyFromStrategy>.FromValue(AssemblyFromStrategy.Releasable(LoadFromAssemblyPath(localFile)), false);
            }

            return ValueOrProceed<AssemblyFromStrategy>.Proceed();
        }

        protected virtual ValueOrProceed<AssemblyFromStrategy> LoadFromRemote(string fullPathToPluginAssembly, AssemblyName assemblyName)
        {
            var assemblyFileName = $"{assemblyName.Name}.dll";
            if (File.Exists(Path.Combine(Path.GetDirectoryName(fullPathToPluginAssembly), assemblyFileName)))
            {
                return LoadDependencyFromLocalDisk(Path.GetDirectoryName(fullPathToPluginAssembly), assemblyFileName);
            }
            return ValueOrProceed<AssemblyFromStrategy>.Proceed();
        }

        protected virtual ValueOrProceed<AssemblyFromStrategy> LoadFromDefaultContext(string fullPathToPluginAssembly, AssemblyName assemblyName)
        {
            try
            {
                var assembly = Default.LoadFromAssemblyName(assemblyName);
                if (assembly != null)
                    return ValueOrProceed<AssemblyFromStrategy>.FromValue(AssemblyFromStrategy.NotReleasable(assembly), false);
            }
            catch (FileNotFoundException) { } // This can happen if the plugin uses a newer version of a package referenced in the host

            var hostAssembly = this.pluginDependencyContext.HostDependencies.FirstOrDefault(h => h.DependencyName.Name == assemblyName.Name);
            if (hostAssembly != null && !hostAssembly.AllowDowngrade)
            {
                if (!!hostAssembly.AllowDowngrade)
                    throw new AssemblyLoadException($"Plugin Assembly reference {assemblyName.Name} with version {assemblyName.Version} was requested but not found in the host. The version from the host is {hostAssembly.DependencyName.Version}. Possible version mismatch. Please downgrade your plugin.");
            }

            return ValueOrProceed<AssemblyFromStrategy>.Proceed();
        }

        protected virtual ValueOrProceed<AssemblyFromStrategy> LoadDependencyFromLocalDisk(string directory, string assemblyFileName)
        {
            var dependency = LoadFileFromLocalDisk(directory, assemblyFileName);

            if (dependency == null)
                return ValueOrProceed<AssemblyFromStrategy>.Proceed();

            return ValueOrProceed<AssemblyFromStrategy>.FromValue(AssemblyFromStrategy.Releasable(Assembly.Load(ToByteArray(dependency))), false);
        }

        internal static Stream LoadFileFromLocalDisk(string loadPath, string pluginAssemblyName)
        {
            var probingPath = EnsureFileExists(loadPath, pluginAssemblyName);
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(probingPath, FileMode.Open, FileAccess.Read))
            {
                memoryStream.SetLength(stream.Length);
                stream.Read(memoryStream.GetBuffer(), 0, (int)stream.Length);
            }
            return memoryStream;
        }

        internal static byte[] ToByteArray(Stream stream)
        {
            stream.Position = 0;
            byte[] buffer = new byte[stream.Length];
            for (int totalBytesCopied = 0; totalBytesCopied < stream.Length;)
                totalBytesCopied += stream.Read(buffer, totalBytesCopied, Convert.ToInt32(stream.Length) - totalBytesCopied);
            return buffer;
        }

        private static string EnsureFileExists(string loadPath, string pluginAssemblyName)
        {
            var probingPath = Path.GetFullPath(Path.Combine(loadPath, pluginAssemblyName));
            if (!File.Exists(probingPath))
                throw new AssemblyLoadException($"Plugin assembly does not exist in path : {probingPath}");
            return probingPath;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            // This fixes the issue where the ALC is still alive and utilized in the host
            if (this.disposed || this.disposing)
                return IntPtr.Zero;

            IntPtr library = IntPtr.Zero;

            var nativeAssembly = assemblyLoadStrategy.LoadUnmanagedDll(
                    this.initialPluginLoadDirectory,
                    unmanagedDllName,
                    LoadUnmanagedFromDependencyContext,
                    LoadUnmanagedFromRemote,
                    LoadUnmanagedFromDefault
                );

            if (!String.IsNullOrEmpty(nativeAssembly.Path))
                // Load via assembly path
                library = LoadUnmanagedDllFromDependencyLookup(Path.GetFullPath(nativeAssembly.Path));
            else
                // Load via provided pointer
                library = nativeAssembly.Pointer;

            if (library != IntPtr.Zero && // If the library was found
                !String.IsNullOrEmpty(nativeAssembly.Path) && // and it was found via the dependency lookup
                !this.loadedNativeLibraries.ContainsKey(nativeAssembly.Path)) // and it was not already loaded
                this.loadedNativeLibraries[nativeAssembly.Path] = library; // Add it to the list in order to have it unloaded later

            return library;
        }

        /// <summary>
        /// This override includes the netcore 3.0 resolver
        /// </summary>
        /// <param name="unmanagedDllName"></param>
        /// <returns></returns>
        protected ValueOrProceed<string> LoadUnmanagedFromDependencyContext(string fullPathToPluginAssembly, string unmanagedDllName)
        {
            string libraryPath = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (!String.IsNullOrEmpty(libraryPath))
            {
                string runtimeCandidate = null;
                if (this.nativeDependencyLoadPreference == NativeDependencyLoadPreference.PreferInstalledRuntime)
                    // Prefer loading from runtime folder
                    runtimeCandidate = this.pluginDependencyResolver.ResolvePlatformDependencyPathToRuntime(this.pluginPlatformVersion, libraryPath);

                return ValueOrProceed<string>.FromValue(runtimeCandidate ?? libraryPath, false);
            }

            var unmanagedDllNameWithoutFileExtension = Path.GetFileNameWithoutExtension(unmanagedDllName);
            var platformDependency = this.pluginDependencyContext.PlatformDependencies.FirstOrDefault(d => d.DependencyNameWithoutExtension == unmanagedDllNameWithoutFileExtension);
            if (platformDependency != null)
            {
                var dependencyPath = Path.GetDirectoryName(fullPathToPluginAssembly);
                var pathToDependency = this.pluginDependencyResolver.ResolvePlatformDependencyToPath(dependencyPath, platformDependency);
                if (!String.IsNullOrEmpty(pathToDependency))
                {
                    string runtimeCandidate = null;
                    if (this.nativeDependencyLoadPreference == NativeDependencyLoadPreference.PreferInstalledRuntime)
                        // Prefer loading from runtime folder
                        runtimeCandidate = this.pluginDependencyResolver.ResolvePlatformDependencyPathToRuntime(this.pluginPlatformVersion, pathToDependency);

                    return ValueOrProceed<string>.FromValue(runtimeCandidate ?? pathToDependency, false);
                }
            }

            return ValueOrProceed<string>.FromValue(String.Empty, true);
        }

        protected virtual ValueOrProceed<string> LoadUnmanagedFromRemote(string fullPathToPluginAssembly, string unmanagedDllName)
        {
            var assemblyFileName = $"{unmanagedDllName}.dll";
            var pathToDependency = Path.Combine(Path.GetDirectoryName(fullPathToPluginAssembly), assemblyFileName);
            if (File.Exists(pathToDependency))
            {
                return ValueOrProceed<string>.FromValue(pathToDependency, false);
            }
            return ValueOrProceed<string>.FromValue(String.Empty, true);
        }

        protected virtual ValueOrProceed<IntPtr> LoadUnmanagedFromDefault(string fullPathToPluginAssembly, string unmanagedDllName)
        {
            var resolution = base.LoadUnmanagedDll(unmanagedDllName);
            if (resolution == default(IntPtr))
                return ValueOrProceed<IntPtr>.Proceed();

            return ValueOrProceed<IntPtr>.FromValue(resolution, false);
        }

        /// <summary>
        /// Load the assembly using the base.LoadUnmanagedDllFromPath functionality 
        /// </summary>
        /// <param name="fullPathToNativeAssembly"></param>
        /// <returns>A loaded native library pointer</returns>
        protected virtual IntPtr LoadUnmanagedDllFromDependencyLookup(string fullPathToNativeAssembly) => base.LoadUnmanagedDllFromPath(fullPathToNativeAssembly);

        public async Task Unload()
        {
            if (this.isCollectible)
                base.Unload();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                this.disposing = true;

                GC.Collect();
                GC.WaitForPendingFinalizers();

                if (this.assemblyReferences != null)
                    foreach (var reference in this.assemblyReferences)
                        // https://docs.microsoft.com/en-us/dotnet/standard/assembly/unloadability#use-collectible-assemblyloadcontext
                        for (int i = 0; reference.IsAlive && (i < 10); i++)
                        {
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }

                this.loadedPlugins.Clear();
                this.loadedPlugins = null;

                this.assemblyReferences.Clear();
                this.assemblyReferences = null;

                // Unload any loaded native assemblies
                foreach (var nativeAssembly in this.loadedNativeLibraries)
                    this.nativeAssemblyUnloader.UnloadNativeAssembly(nativeAssembly.Key, nativeAssembly.Value);

                this.loadedNativeLibraries = null;
                this.nativeAssemblyUnloader = null;
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected Assembly LoadAndAddToWeakReferences(AssemblyFromStrategy assemblyFromStrategy)
        {
            if (assemblyFromStrategy != null && assemblyFromStrategy.CanBeReleased)
                this.assemblyReferences.Add(new System.WeakReference(assemblyFromStrategy.Assembly));

            return assemblyFromStrategy?.Assembly;
        }

        protected Assembly LoadAndAddToWeakReferences(Stream stream)
        {
            var assembly = base.LoadFromStream(stream); // ==> AssemblyLoadContext.LoadFromStream(Stream stream)
            this.assemblyReferences.Add(new System.WeakReference(assembly));
            return assembly;
        }

        internal static async Task<Stream> LoadFileFromLocalDisk(string fullPathToAssembly)
        {
            var probingPath = EnsureFileExists(fullPathToAssembly);
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(probingPath, FileMode.Open, FileAccess.Read))
            {
                memoryStream.SetLength(stream.Length);
                await stream.ReadAsync(memoryStream.GetBuffer(), 0, (int)stream.Length);
            }
            return memoryStream;
        }

        private static string EnsureFileExists(string fullPathToAssembly)
        {
            var probingPath = Path.GetFullPath(fullPathToAssembly);
            if (!File.Exists(probingPath))
                throw new AssemblyLoadException($"Plugin assembly does not exist in path : {probingPath}");
            return probingPath;
        }
    }

    public class DefaultAssemblyLoader : IPluginAssemblyLoader, IDisposable
    {
        protected ConcurrentDictionary<string, IAssemblyLoadContext> loadContexts;
        protected ConcurrentDictionary<string, WeakReference> loadContextReferences;
        protected bool disposed = false;

        public DefaultAssemblyLoader()
        {
            this.loadContexts = new ConcurrentDictionary<string, IAssemblyLoadContext>();
            this.loadContextReferences = new ConcurrentDictionary<string, WeakReference>();
        }

        public virtual Task<Assembly> Load(string fullPathToAssembly)
        {
            if (!Path.IsPathRooted(fullPathToAssembly))
                throw new AssemblyLoadException($"fullPathToAssembly {fullPathToAssembly} is not rooted, this must be a absolute path!");

            var loadContext = new DefaultAssemblyLoadContext();
            this.loadContexts[fullPathToAssembly] = loadContext;
            this.loadContextReferences[fullPathToAssembly] = new System.WeakReference(loadContext);
            return loadContext.LoadPluginAssembly(fullPathToAssembly);
        }

        public virtual async Task Unload(string fullPathToAssembly)
        {
            UnloadContext(fullPathToAssembly);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                DisposeAndUnloadContexts();
            }
            this.disposed = true;
        }

        protected virtual void UnloadContext(string fullPathToAssembly)
        {
            var pluginName = Path.GetFileNameWithoutExtension(fullPathToAssembly);
            var loadContextKeys = this.loadContexts.Keys.Where(k => k == fullPathToAssembly);

            foreach (var key in loadContextKeys)
            {
                var loadContext = this.loadContexts[key];
                loadContext.Unload();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        protected virtual void DisposeAndUnloadContexts()
        {
            if (loadContexts != null)
                foreach (var key in loadContexts.Keys)
                {
                    var loadContext = this.loadContexts[key];
                    loadContext.Unload();
                }

            this.loadContexts.Clear();
            this.loadContexts = null;

            if (this.loadContextReferences != null)
                foreach (var reference in this.loadContextReferences.Values)
                {
                    // https://docs.microsoft.com/en-us/dotnet/standard/assembly/unloadability#use-collectible-assemblyloadcontext
                    for (int i = 0; reference.IsAlive && (i < 10); i++)
                    {
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                        GC.WaitForPendingFinalizers();
                    }
                }

            this.loadContextReferences.Clear();
            this.loadContextReferences = null;

            GC.Collect(); // collects all unused memory
            GC.WaitForPendingFinalizers(); // wait until GC has finished its work
            GC.Collect();
        }
    }
}