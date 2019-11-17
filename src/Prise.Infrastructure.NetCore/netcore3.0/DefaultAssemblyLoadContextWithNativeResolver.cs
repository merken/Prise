using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace Prise.Infrastructure.NetCore
{
    public class DefaultAssemblyLoadContextWithNativeResolver : DefaultAssemblyLoadContext
    {
        protected AssemblyDependencyResolver resolver;

        public DefaultAssemblyLoadContextWithNativeResolver(
            IHostFrameworkProvider hostFrameworkProvider,
            IRootPathProvider rootPathProvider,
            ILocalAssemblyLoaderOptions options,
            IHostTypesProvider hostTypesProvider,
            IRemoteTypesProvider remoteTypesProvider,
            IDependencyPathProvider dependencyPathProvider,
            IProbingPathsProvider probingPathsProvider,
            IRuntimePlatformContext runtimePlatformContext,
            IDepsFileProvider depsFileProvider,
            IPluginDependencyResolver pluginDependencyResolver,
            INativeAssemblyUnloader nativeAssemblyUnloader
        ) : base(
                options.PluginPlatformVersion,
                options.DependencyLoadPreference,
                options.NativeDependencyLoadPreference,
                hostFrameworkProvider,
                rootPathProvider,
                hostTypesProvider,
                remoteTypesProvider,
                dependencyPathProvider,
                probingPathsProvider,
                runtimePlatformContext,
                depsFileProvider,
                pluginDependencyResolver,
                nativeAssemblyUnloader,
                options.PluginPath
            )
        { }

        public override Assembly LoadPluginAssembly(string pluginAssemblyName)
        {
            // contains rootpath + plugin folder + plugin assembly name
            // HostApplication/bin/Debug/netcoreapp3.0 + Plugins + Plugin.dll
            this.resolver = new AssemblyDependencyResolver(Path.Join(this.pluginPath, pluginAssemblyName));
            return base.LoadPluginAssembly(pluginAssemblyName);
        }

        public override Task<Assembly> LoadPluginAssemblyAsync(string pluginAssemblyName)
        {
            // contains rootpath + plugin folder + plugin assembly name
            // HostApplication/bin/Debug/netcoreapp3.0 + Plugins + Plugin.dll
            this.resolver = new AssemblyDependencyResolver(Path.Join(this.pluginPath, pluginAssemblyName));
            return base.LoadPluginAssemblyAsync(pluginAssemblyName);
        }

        /// <summary>
        /// This override includes the netcore 3.0 resolver
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        protected override ValueOrProceed<Assembly> LoadFromDependencyContext(AssemblyName assemblyName)
        {
            var assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
            if (!String.IsNullOrEmpty(assemblyPath) && File.Exists(assemblyPath))
            {
                return ValueOrProceed<Assembly>.FromValue(LoadFromAssemblyPath(assemblyPath), false);
            }

            return base.LoadFromDependencyContext(assemblyName);
        }

        /// <summary>
        /// This override includes the netcore 3.0 resolver
        /// </summary>
        /// <param name="unmanagedDllName"></param>
        /// <returns></returns>
        protected override ValueOrProceed<string> LoadUnmanagedFromDependencyContext(string unmanagedDllName)
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

            return base.LoadUnmanagedFromDependencyContext(unmanagedDllName);
        }

        /// <summary>
        /// Load the assembly using the .NET Core 3 System.Runtime.InteropServices.NativeLibrary functionality 
        /// </summary>
        /// <param name="fullPathToNativeAssembly"></param>
        /// <returns>A loaded native library pointer</returns>
        protected override IntPtr LoadUnmanagedDllFromDependencyLookup(string fullPathToNativeAssembly) => System.Runtime.InteropServices.NativeLibrary.Load(fullPathToNativeAssembly);

        protected override void Dispose(bool disposing)
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
                this.resolver = null;

                foreach (var nativeAssembly in this.loadedNativeLibraries)
                    this.nativeAssemblyUnloader.UnloadNativeAssembly(nativeAssembly.Key, nativeAssembly.Value);

                this.nativeAssemblyUnloader = null;
                this.loadedNativeLibraries = null;
                this.rootPath = null;
                this.pluginPath = null;

                foreach (var ctx in AssemblyLoadContext.All)
                {
                    if (ctx.IsCollectible)
                        ctx.Unload();
                }
            }
            this.disposed = true;
        }
    }
}