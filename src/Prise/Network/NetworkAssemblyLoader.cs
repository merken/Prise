using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Prise.Infrastructure;

namespace Prise
{
    public class NetworkAssemblyLoader<T> : DisposableAssemblyUnLoader, IPluginAssemblyLoader<T>
    {
        private readonly IPluginLogger<T> logger;
        private readonly INetworkAssemblyLoaderOptions<T> options;
        private readonly IHostFrameworkProvider hostFrameworkProvider;
        private readonly IHostTypesProvider<T> hostTypesProvider;
        private readonly IDowngradableDependenciesProvider<T> downgradableDependenciesProvider;
        private readonly IRemoteTypesProvider<T> remoteTypesProvider;
        private readonly IDependencyPathProvider<T> dependencyPathProvider;
        private readonly IProbingPathsProvider<T> probingPathsProvider;
        private readonly IRuntimePlatformContext runtimePlatformContext;
        private readonly IDepsFileProvider<T> depsFileProvider;
        private readonly IPluginDependencyResolver<T> pluginDependencyResolver;
        private readonly INativeAssemblyUnloader nativeAssemblyUnloader;
        private readonly IAssemblyLoadStrategyProvider assemblyLoadStrategyProvider;
        private readonly ITempPathProvider<T> tempPathProvider;
        private readonly IHttpClientFactory httpClientFactory;

        public NetworkAssemblyLoader(
            IPluginLogger<T> logger,
            INetworkAssemblyLoaderOptions<T> options,
            IHostFrameworkProvider hostFrameworkProvider,
            IHostTypesProvider<T> hostTypesProvider,
            IDowngradableDependenciesProvider<T> downgradableDependenciesProvider,
            IRemoteTypesProvider<T> remoteTypesProvider,
            IDependencyPathProvider<T> dependencyPathProvider,
            IProbingPathsProvider<T> probingPathsProvider,
            IRuntimePlatformContext runtimePlatformContext,
            IDepsFileProvider<T> depsFileProvider,
            IPluginDependencyResolver<T> pluginDependencyResolver,
            INativeAssemblyUnloader nativeAssemblyUnloader,
            IAssemblyLoadStrategyProvider assemblyLoadStrategyProvider,
            ITempPathProvider<T> tempPathProvider,
            IHttpClientFactory httpClientFactory
            ) : base()
        {
            this.logger = logger;
            this.options = options;
            this.hostFrameworkProvider = hostFrameworkProvider;
            this.hostTypesProvider = hostTypesProvider;
            this.downgradableDependenciesProvider = downgradableDependenciesProvider;
            this.remoteTypesProvider = remoteTypesProvider;
            this.dependencyPathProvider = dependencyPathProvider;
            this.probingPathsProvider = probingPathsProvider;
            this.runtimePlatformContext = runtimePlatformContext;
            this.depsFileProvider = depsFileProvider;
            this.pluginDependencyResolver = pluginDependencyResolver;
            this.nativeAssemblyUnloader = nativeAssemblyUnloader;
            this.assemblyLoadStrategyProvider = assemblyLoadStrategyProvider;
            this.tempPathProvider = tempPathProvider;
            this.httpClientFactory = httpClientFactory;
        }

        public virtual Assembly Load(IPluginLoadContext pluginLoadContext)
        {
            var loadContext = new NetworkAssemblyLoadContext<T>(
               this.logger,
               this.options,
               this.hostFrameworkProvider,
               this.hostTypesProvider,
               this.downgradableDependenciesProvider,
               this.remoteTypesProvider,
               this.dependencyPathProvider,
               this.probingPathsProvider,
               this.runtimePlatformContext,
               this.depsFileProvider,
               this.pluginDependencyResolver,
               this.nativeAssemblyUnloader,
               this.assemblyLoadStrategyProvider,
               this.httpClientFactory,
               this.tempPathProvider
           );

            var loadedPluginKey = new LoadedPluginKey(pluginLoadContext);
            this.loadContexts[loadedPluginKey] = loadContext;
            this.loadContextReferences[loadedPluginKey] = new System.WeakReference(loadContext);

            return loadContext.LoadPluginAssembly(pluginLoadContext);
        }

        public virtual Task<Assembly> LoadAsync(IPluginLoadContext pluginLoadContext)
        {
            var loadContext = new NetworkAssemblyLoadContext<T>(
               this.logger,
               this.options,
               this.hostFrameworkProvider,
               this.hostTypesProvider,
               this.downgradableDependenciesProvider,
               this.remoteTypesProvider,
               this.dependencyPathProvider,
               this.probingPathsProvider,
               this.runtimePlatformContext,
               this.depsFileProvider,
               this.pluginDependencyResolver,
               this.nativeAssemblyUnloader,
               this.assemblyLoadStrategyProvider,
               this.httpClientFactory,
               this.tempPathProvider
           );

            var loadedPluginKey = new LoadedPluginKey(pluginLoadContext);
            this.loadContexts[loadedPluginKey] = loadContext;
            this.loadContextReferences[loadedPluginKey] = new System.WeakReference(loadContext);

            return loadContext.LoadPluginAssemblyAsync(pluginLoadContext);
        }
    }
}
