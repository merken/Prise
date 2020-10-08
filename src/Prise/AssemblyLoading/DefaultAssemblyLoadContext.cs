using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Prise.Core;
using Prise.Utils;

namespace Prise.AssemblyLoading
{
    public class DefaultAssemblyLoadContext : InMemoryAssemblyLoadContext, IAssemblyLoadContext
    {
        private readonly IServiceProvider serviceProvider;
        protected IAssemblyDependencyResolver resolver;
        protected ConcurrentDictionary<string, IntPtr> loadedNativeLibraries;
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
        protected Func<IPluginDependencyResolver> pluginDependencyResolverFactory;
        protected Func<IAssemblyLoadStrategy> assemblyLoadStrategyFactory;
        protected Func<IPluginLoadContext, Task<IPluginDependencyContext>> pluginDependencyContextFactory;
        protected Func<string, IAssemblyDependencyResolver> assemblyDependencyResolverFactory;
        protected IFileSystemUtilities fileSystemUtilities;

        public DefaultAssemblyLoadContext(Func<INativeAssemblyUnloader> nativeAssemblyUnloaderFactory,
                                          Func<IPluginDependencyResolver> pluginDependencyResolverFactory,
                                          Func<IAssemblyLoadStrategy> assemblyLoadStrategyFactory,
                                          Func<IPluginLoadContext, Task<IPluginDependencyContext>> pluginDependencyContextFactory,
                                          Func<string, IAssemblyDependencyResolver> assemblyDependencyResolverFactory,
                                          Func<IFileSystemUtilities> fileSystemUtilitiesFactory)
        {
            this.nativeAssemblyUnloader = nativeAssemblyUnloaderFactory.ThrowIfNull(nameof(nativeAssemblyUnloaderFactory))();
            this.pluginDependencyResolverFactory = pluginDependencyResolverFactory.ThrowIfNull(nameof(pluginDependencyResolverFactory));
            this.assemblyLoadStrategyFactory = assemblyLoadStrategyFactory.ThrowIfNull(nameof(assemblyLoadStrategyFactory));
            this.pluginDependencyContextFactory = pluginDependencyContextFactory.ThrowIfNull(nameof(pluginDependencyContextFactory));
            this.assemblyDependencyResolverFactory = assemblyDependencyResolverFactory.ThrowIfNull(nameof(assemblyDependencyResolverFactory));
            this.fileSystemUtilities = fileSystemUtilitiesFactory.ThrowIfNull(nameof(fileSystemUtilitiesFactory))();
            this.loadedNativeLibraries = new ConcurrentDictionary<string, IntPtr>();
            this.loadedPlugins = new ConcurrentBag<string>();
            this.assemblyReferences = new ConcurrentBag<WeakReference>();
        }

        private void GuardIfAlreadyLoaded(string pluginAssemblyName)
        {
            if (this.disposed || this.disposing)
                throw new AssemblyLoadingException($"Cannot load Plugin {pluginAssemblyName} when disposed.");

            if (String.IsNullOrEmpty(pluginAssemblyName))
                throw new AssemblyLoadingException($"Cannot load empty Plugin. {nameof(pluginAssemblyName)} was null or empty.");

            if (this.loadedPlugins.Contains(pluginAssemblyName))
                throw new AssemblyLoadingException($"Plugin {pluginAssemblyName} was already loaded.");

            this.loadedPlugins.Add(pluginAssemblyName);
        }

        public async Task<IAssemblyShim> LoadPluginAssembly(IPluginLoadContext pluginLoadContext)
        {
            if (pluginLoadContext == null)
                throw new ArgumentNullException(nameof(pluginLoadContext));

            var fullPathToAssembly = pluginLoadContext.FullPathToPluginAssembly.ThrowIfNullOrEmpty(nameof(pluginLoadContext.FullPathToPluginAssembly));

            if (!Path.IsPathRooted(fullPathToAssembly))
                throw new AssemblyLoadingException($"FullPathToPluginAssembly {pluginLoadContext.FullPathToPluginAssembly} is not rooted, this must be a absolute path!");

            this.fullPathToPluginAssembly = pluginLoadContext.FullPathToPluginAssembly;
            this.initialPluginLoadDirectory = Path.GetDirectoryName(fullPathToPluginAssembly);

            GuardIfAlreadyLoaded(fullPathToPluginAssembly);

            this.resolver = this.assemblyDependencyResolverFactory(fullPathToPluginAssembly);
            this.pluginDependencyContext = await this.pluginDependencyContextFactory(pluginLoadContext);
            this.pluginDependencyResolver = this.pluginDependencyResolverFactory();
            this.assemblyLoadStrategy = this.assemblyLoadStrategyFactory();
            this.pluginPlatformVersion = pluginLoadContext.PluginPlatformVersion ?? PluginPlatformVersion.Empty();

            var ensuredPath = this.fileSystemUtilities.EnsureFileExists(fullPathToPluginAssembly);

            using (var pluginStream = await this.fileSystemUtilities.ReadFileFromDisk(ensuredPath))
            {
                return new PriseAssembly(LoadAndAddToWeakReferences(pluginStream));
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
                    this.pluginDependencyContext,
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
        protected ValueOrProceed<AssemblyFromStrategy> LoadFromDependencyContext(string initialPluginLoadDirectory, AssemblyName assemblyName)
        {
            var assemblyPath = this.resolver.ResolveAssemblyToPath(assemblyName);
            if (!String.IsNullOrEmpty(assemblyPath) && this.fileSystemUtilities.DoesFileExist(assemblyPath))
            {
                return ValueOrProceed<AssemblyFromStrategy>.FromValue(AssemblyFromStrategy.Releasable(LoadFromAssemblyPath(assemblyPath)), false);
            }

            if (IsResourceAssembly(assemblyName))
            {
                foreach (var resourceDependency in this.pluginDependencyContext.PluginResourceDependencies)
                {
                    var resourcePath = Path.Combine(resourceDependency.Path, assemblyName.CultureName, assemblyName.Name + ".dll");
                    if (this.fileSystemUtilities.DoesFileExist(resourcePath))
                    {
                        return ValueOrProceed<AssemblyFromStrategy>.FromValue(AssemblyFromStrategy.Releasable(LoadFromAssemblyPath(resourcePath)), false);
                    }
                }

                // Do not proceed probing
                return ValueOrProceed<AssemblyFromStrategy>.FromValue(null, false);
            }

            var pluginDependency = this.pluginDependencyContext.PluginDependencies.FirstOrDefault(d => d.DependencyNameWithoutExtension == assemblyName.Name);
            if (pluginDependency != null)
            {
                var dependency = this.pluginDependencyResolver.ResolvePluginDependencyToPath(initialPluginLoadDirectory, pluginDependency, this.pluginDependencyContext.AdditionalProbingPaths);
                if (dependency != null)
                    return ValueOrProceed<AssemblyFromStrategy>.FromValue(AssemblyFromStrategy.Releasable(LoadFromStream(dependency)), false);
            }

            var localFile = Path.Combine(initialPluginLoadDirectory, assemblyName.Name + ".dll");
            if (this.fileSystemUtilities.DoesFileExist(localFile))
            {
                return ValueOrProceed<AssemblyFromStrategy>.FromValue(AssemblyFromStrategy.Releasable(LoadFromAssemblyPath(localFile)), false);
            }

            return ValueOrProceed<AssemblyFromStrategy>.Proceed();
        }

        protected virtual ValueOrProceed<AssemblyFromStrategy> LoadFromRemote(string initialPluginLoadDirectory, AssemblyName assemblyName)
        {
            var assemblyFileName = $"{assemblyName.Name}.dll";
            if (this.fileSystemUtilities.DoesFileExist(Path.Combine(initialPluginLoadDirectory, assemblyFileName)))
            {
                return LoadDependencyFromLocalDisk(initialPluginLoadDirectory, assemblyFileName);
            }
            return ValueOrProceed<AssemblyFromStrategy>.Proceed();
        }

        protected virtual ValueOrProceed<AssemblyFromStrategy> LoadFromDefaultContext(string initialPluginLoadDirectory, AssemblyName assemblyName)
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
                if (!hostAssembly.AllowDowngrade)
                    throw new AssemblyLoadingException($"Plugin Assembly reference {assemblyName.Name} with version {assemblyName.Version} was requested but not found in the host. The version from the host is {hostAssembly.DependencyName.Version}. Possible version mismatch. Please downgrade your plugin or add {assemblyName.Name} to downgradableHostAssemblies.");
            }

            return ValueOrProceed<AssemblyFromStrategy>.Proceed();
        }

        protected virtual ValueOrProceed<AssemblyFromStrategy> LoadDependencyFromLocalDisk(string directory, string assemblyFileName)
        {
            var dependency = this.fileSystemUtilities.ReadDependencyFileFromDisk(directory, assemblyFileName);

            if (dependency == null)
                return ValueOrProceed<AssemblyFromStrategy>.Proceed();

            return ValueOrProceed<AssemblyFromStrategy>.FromValue(AssemblyFromStrategy.Releasable(Assembly.Load(this.fileSystemUtilities.ToByteArray(dependency))), false);
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
                    this.pluginDependencyContext,
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
        protected ValueOrProceed<string> LoadUnmanagedFromDependencyContext(string initialPluginLoadDirectory, string unmanagedDllName)
        {
            string libraryPath = this.resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
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
                var pathToDependency = this.pluginDependencyResolver.ResolvePlatformDependencyToPath(initialPluginLoadDirectory, platformDependency, this.pluginDependencyContext.AdditionalProbingPaths);
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

        protected virtual ValueOrProceed<string> LoadUnmanagedFromRemote(string initialPluginLoadDirectory, string unmanagedDllName)
        {
            var assemblyFileName = $"{unmanagedDllName}.dll";
            var pathToDependency = Path.Combine(initialPluginLoadDirectory, assemblyFileName);
            if (this.fileSystemUtilities.DoesFileExist(pathToDependency))
            {
                return ValueOrProceed<string>.FromValue(pathToDependency, false);
            }
            return ValueOrProceed<string>.FromValue(String.Empty, true);
        }

        protected virtual ValueOrProceed<IntPtr> LoadUnmanagedFromDefault(string initialPluginLoadDirectory, string unmanagedDllName)
        {
            var resolution = base.LoadUnmanagedDll(unmanagedDllName);
            if (resolution == default(IntPtr))
                return ValueOrProceed<IntPtr>.Proceed();

            return ValueOrProceed<IntPtr>.FromValue(resolution, false);
        }

        /// <summary>
        /// Load the assembly using the base.LoadUnmanagedDllFromPath functionality 
        /// </summary>
        /// <param name="fullPathToUnmanagedDll"></param>
        /// <returns>A loaded native library pointer</returns>
        protected virtual IntPtr LoadUnmanagedDllFromDependencyLookup(string fullPathToUnmanagedDll) => base.LoadUnmanagedDllFromPath(fullPathToUnmanagedDll);

        public async Task Unload()
        {
#if SUPPORTS_UNLOADING
            if (this.isCollectible)
                base.Unload();
#endif
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


                // Unload any loaded native assemblies
                foreach (var nativeAssembly in this.loadedNativeLibraries)
                    this.nativeAssemblyUnloader.UnloadNativeAssembly(nativeAssembly.Key, nativeAssembly.Value);

                this.loadedPlugins.Clear();
                this.assemblyReferences.Clear();
                this.loadedNativeLibraries.Clear();
                this.pluginDependencyContext?.Dispose();
                this.pluginDependencyResolver?.Dispose();
                this.resolver?.Dispose();
                this.resolver = null;
                this.loadedNativeLibraries = null;
                this.loadedPlugins = null;
                this.assemblyReferences = null;
                this.assemblyLoadStrategy = null;
                this.pluginDependencyContext = null;
                this.pluginDependencyResolver = null;
                this.fullPathToPluginAssembly = null;
                this.initialPluginLoadDirectory = null;
                this.pluginPlatformVersion = null;
                this.nativeAssemblyUnloader = null;
                this.pluginDependencyResolverFactory = null;
                this.assemblyLoadStrategyFactory = null;
                this.pluginDependencyContextFactory = null;
                this.assemblyDependencyResolverFactory = null;
                this.fileSystemUtilities = null;
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