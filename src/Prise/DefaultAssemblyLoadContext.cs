using Prise.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise
{
    public class DefaultAssemblyLoadContext : InMemoryAssemblyLoadContext, IAssemblyLoadContext
    {
        protected string pluginPath;
        protected IHostFrameworkProvider hostFrameworkProvider;
        protected IHostTypesProvider hostTypesProvider;
        protected IRemoteTypesProvider remoteTypesProvider;
        protected IDependencyPathProvider dependencyPathProvider;
        protected IProbingPathsProvider probingPathsProvider;
        protected IRuntimePlatformContext runtimePlatformContext;
        protected IDepsFileProvider depsFileProvider;
        protected IPluginDependencyResolver pluginDependencyResolver;
        protected IPluginDependencyContext pluginDependencyContext;
        protected INativeAssemblyUnloader nativeAssemblyUnloader;
        internal IAssemblyLoadStrategy assemblyLoadStrategy;
        internal IAssemblyLoadOptions options;
        protected bool disposed = false;
        protected ConcurrentDictionary<string, IntPtr> loadedNativeLibraries;

        public DefaultAssemblyLoadContext(
            IAssemblyLoadOptions options,
            IHostFrameworkProvider hostFrameworkProvider,
            IPluginPathProvider pluginPathProvider,
            IHostTypesProvider hostTypesProvider,
            IRemoteTypesProvider remoteTypesProvider,
            IDependencyPathProvider dependencyPathProvider,
            IProbingPathsProvider probingPathsProvider,
            IRuntimePlatformContext runtimePlatformContext,
            IDepsFileProvider depsFileProvider,
            IPluginDependencyResolver pluginDependencyResolver,
            INativeAssemblyUnloader nativeAssemblyUnloader)
        {
            this.options = options;
            this.pluginPath = pluginPathProvider.GetPluginPath();
            this.hostFrameworkProvider = hostFrameworkProvider;
            this.hostTypesProvider = hostTypesProvider;
            this.remoteTypesProvider = remoteTypesProvider;
            this.dependencyPathProvider = dependencyPathProvider;
            this.probingPathsProvider = probingPathsProvider;
            this.runtimePlatformContext = runtimePlatformContext;
            this.depsFileProvider = depsFileProvider;
            this.pluginDependencyResolver = pluginDependencyResolver;
            this.nativeAssemblyUnloader = nativeAssemblyUnloader;
            this.loadedNativeLibraries = new ConcurrentDictionary<string, IntPtr>();
        }

        public virtual Assembly LoadPluginAssembly(string pluginAssemblyName)
        {
            if (this.pluginDependencyContext != null)
                throw new PrisePluginException($"Plugin {pluginAssemblyName} was already loaded");

            this.pluginDependencyContext = PluginDependencyContext.FromPluginAssembly(
                pluginAssemblyName,
                this.hostFrameworkProvider,
                this.hostTypesProvider.ProvideHostTypes(),
                this.remoteTypesProvider.ProvideRemoteTypes(),
                this.runtimePlatformContext,
                this.depsFileProvider,
                this.options.IgnorePlatformInconsistencies);

            using (var pluginStream = LoadFileFromLocalDisk(pluginPath, pluginAssemblyName))
            {
                this.assemblyLoadStrategy = AssemblyLoadStrategyFactory
                  .GetAssemblyLoadStrategy(this.options.DependencyLoadPreference, this.pluginDependencyContext);

                return base.LoadFromStream(pluginStream); // ==> AssemblyLoadContext.LoadFromStream(Stream stream);
            }
        }

        public virtual async Task<Assembly> LoadPluginAssemblyAsync(string pluginAssemblyName)
        {
            if (this.pluginDependencyContext != null)
                throw new PrisePluginException($"Plugin {pluginAssemblyName} was already loaded");

            this.pluginDependencyContext = await PluginDependencyContext.FromPluginAssemblyAsync(
                pluginAssemblyName,
                this.hostFrameworkProvider,
                this.hostTypesProvider.ProvideHostTypes(),
                this.remoteTypesProvider.ProvideRemoteTypes(),
                this.runtimePlatformContext,
                this.depsFileProvider,
                this.options.IgnorePlatformInconsistencies);

            using (var pluginStream = await LoadFileFromLocalDiskAsync(pluginPath, pluginAssemblyName))
            {
                this.assemblyLoadStrategy = AssemblyLoadStrategyFactory
                  .GetAssemblyLoadStrategy(this.options.DependencyLoadPreference, this.pluginDependencyContext);

                return base.LoadFromStream(pluginStream); // ==> AssemblyLoadContext.LoadFromStream(Stream stream);
            }
        }

        protected virtual ValueOrProceed<Assembly> LoadFromDefaultContext(AssemblyName assemblyName)
        {
            try
            {
                var assembly = Default.LoadFromAssemblyName(assemblyName);
                if (assembly != null)
                    return ValueOrProceed<Assembly>.FromValue(assembly, false);
            }
            catch (FileNotFoundException) { }

            return ValueOrProceed<Assembly>.Proceed();
        }

        protected virtual ValueOrProceed<Assembly> LoadFromRemote(AssemblyName assemblyName)
        {
            var assemblyFileName = $"{assemblyName.Name}.dll";
            if (File.Exists(Path.Combine(this.pluginPath, assemblyFileName)))
            {
                return LoadDependencyFromLocalDisk(assemblyFileName);
            }
            return ValueOrProceed<Assembly>.Proceed();
        }

        protected virtual bool IsResourceAssembly(AssemblyName assemblyName)
        {
            return !string.IsNullOrEmpty(assemblyName.CultureName) && !string.Equals("neutral", assemblyName.CultureName);
        }

        protected virtual ValueOrProceed<Assembly> LoadFromDependencyContext(AssemblyName assemblyName)
        {
            if (IsResourceAssembly(assemblyName))
            {
                foreach (var resourceDependency in this.pluginDependencyContext.PluginResourceDependencies)
                {
                    var resourcePath = Path.Combine(resourceDependency.Path, assemblyName.CultureName, assemblyName.Name + ".dll");
                    if (File.Exists(resourcePath))
                    {
                        return ValueOrProceed<Assembly>.FromValue(LoadFromAssemblyPath(resourcePath), false);
                    }
                }

                // Do not proceed probing
                return ValueOrProceed<Assembly>.FromValue(null, false);
            }

            var pluginDependency = this.pluginDependencyContext.PluginDependencies.FirstOrDefault(d => d.DependencyNameWithoutExtension == assemblyName.Name);
            if (pluginDependency != null)
            {
                var dependencyPath = this.dependencyPathProvider.GetDependencyPath();
                var probingPaths = this.probingPathsProvider.GetProbingPaths();
                var dependency = this.pluginDependencyResolver.ResolvePluginDependencyToPath(dependencyPath, probingPaths, pluginDependency);
                if (dependency != null)
                    return ValueOrProceed<Assembly>.FromValue(LoadFromStream(dependency), false);
            }

            var localFile = Path.Combine(this.dependencyPathProvider.GetDependencyPath(), assemblyName.Name + ".dll");
            if (File.Exists(localFile))
            {
                return ValueOrProceed<Assembly>.FromValue(LoadFromAssemblyPath(localFile), false);
            }

            return ValueOrProceed<Assembly>.Proceed();
        }

        protected virtual ValueOrProceed<string> LoadUnmanagedFromDependencyContext(string unmanagedDllName)
        {
            var unmanagedDllNameWithoutFileExtension = Path.GetFileNameWithoutExtension(unmanagedDllName);
            var platformDependency = this.pluginDependencyContext.PlatformDependencies.FirstOrDefault(d => d.DependencyNameWithoutExtension == unmanagedDllNameWithoutFileExtension);
            if (platformDependency != null)
            {
                var dependencyPath = this.dependencyPathProvider.GetDependencyPath();
                var probingPaths = this.probingPathsProvider.GetProbingPaths();
                var pathToDependency = this.pluginDependencyResolver.ResolvePlatformDependencyToPath(dependencyPath, probingPaths, platformDependency);
                if (!String.IsNullOrEmpty(pathToDependency))
                {
                    string runtimeCandidate = null;
                    if (this.options.NativeDependencyLoadPreference == NativeDependencyLoadPreference.PreferInstalledRuntime)
                        // Prefer loading from runtime folder
                        runtimeCandidate = this.pluginDependencyResolver.ResolvePlatformDependencyPathToRuntime(this.options.PluginPlatformVersion, pathToDependency);

                    return ValueOrProceed<string>.FromValue(runtimeCandidate ?? pathToDependency, false);
                }
            }

            return ValueOrProceed<string>.FromValue(String.Empty, true);
        }

        protected virtual ValueOrProceed<string> LoadUnmanagedFromRemote(string unmanagedDllName)
        {
            var assemblyFileName = $"{unmanagedDllName}.dll";
            var pathToDependency = Path.Combine(this.pluginPath, assemblyFileName);
            if (File.Exists(pathToDependency))
            {
                return ValueOrProceed<string>.FromValue(pathToDependency, false);
            }
            return ValueOrProceed<string>.FromValue(String.Empty, true);
        }

        protected virtual ValueOrProceed<IntPtr> LoadUnmanagedFromDefault(string unmanagedDllName)
        {
            var resolution = base.LoadUnmanagedDll(unmanagedDllName);
            if (resolution == default(IntPtr))
                return ValueOrProceed<IntPtr>.Proceed();

            return ValueOrProceed<IntPtr>.FromValue(resolution, false);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return assemblyLoadStrategy.LoadAssembly(
                    assemblyName,
                    LoadFromDependencyContext,
                    LoadFromRemote,
                    LoadFromDefaultContext
                );
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            IntPtr library = IntPtr.Zero;

            var nativeAssembly = assemblyLoadStrategy.LoadUnmanagedDll(
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
        /// Load the assembly using the base.LoadUnmanagedDllFromPath functionality 
        /// </summary>
        /// <param name="fullPathToNativeAssembly"></param>
        /// <returns>A loaded native library pointer</returns>
        protected virtual IntPtr LoadUnmanagedDllFromDependencyLookup(string fullPathToNativeAssembly) => base.LoadUnmanagedDllFromPath(fullPathToNativeAssembly);

        protected virtual ValueOrProceed<Assembly> LoadDependencyFromLocalDisk(string assemblyFileName)
        {
            var dependency = LoadFileFromLocalDisk(this.pluginPath, assemblyFileName);

            if (dependency == null)
                return ValueOrProceed<Assembly>.Proceed();

            return ValueOrProceed<Assembly>.FromValue(Assembly.Load(ToByteArray(dependency)), false);
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

        internal static async Task<Stream> LoadFileFromLocalDiskAsync(string loadPath, string pluginAssemblyName)
        {
            var probingPath = EnsureFileExists(loadPath, pluginAssemblyName);
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(probingPath, FileMode.Open, FileAccess.Read))
            {
                memoryStream.SetLength(stream.Length);
                await stream.ReadAsync(memoryStream.GetBuffer(), 0, (int)stream.Length);
            }
            return memoryStream;
        }

        private static string EnsureFileExists(string loadPath, string pluginAssemblyName)
        {
            var probingPath = Path.GetFullPath(Path.Combine(loadPath, pluginAssemblyName));
            if (!File.Exists(probingPath))
                throw new PrisePluginException($"Plugin assembly does not exist in path : {probingPath}");
            return probingPath;
        }

        internal static byte[] ToByteArray(Stream stream)
        {
            stream.Position = 0;
            byte[] buffer = new byte[stream.Length];
            for (int totalBytesCopied = 0; totalBytesCopied < stream.Length;)
                totalBytesCopied += stream.Read(buffer, totalBytesCopied, Convert.ToInt32(stream.Length) - totalBytesCopied);
            return buffer;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                this.hostTypesProvider = null;
                this.remoteTypesProvider = null;
                this.dependencyPathProvider = null;
                this.probingPathsProvider = null;
                this.runtimePlatformContext = null;
                this.depsFileProvider = null;
                this.pluginDependencyResolver = null;
                this.pluginDependencyContext = null;
                this.assemblyLoadStrategy = null;

                // Unload any loaded native assemblies
                foreach (var nativeAssembly in this.loadedNativeLibraries)
                    this.nativeAssemblyUnloader.UnloadNativeAssembly(nativeAssembly.Key, nativeAssembly.Value);

                this.loadedNativeLibraries = null;
                this.nativeAssemblyUnloader = null;
                this.pluginPath = null;
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

#if NETCOREAPP2_1
        public void Unload()
        {
            // What to do for unloading in NETCOREAPP2_1?
            // ==> Nothing, this is only available in .NET Core 3.0+
        }
#endif
    }
}
