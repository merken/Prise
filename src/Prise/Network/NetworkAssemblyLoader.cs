using Prise.Infrastructure;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise
{
    public class NetworkAssemblyLoader<T> : DisposableAssemblyUnLoader, IPluginAssemblyLoader<T>
    {
        private readonly IPluginLogger<T> logger;
        private readonly INetworkAssemblyLoaderOptions<T> options;
        private readonly IHostFrameworkProvider hostFrameworkProvider;
        private readonly IHostTypesProvider<T> hostTypesProvider;
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
            )
        {
            this.logger = logger;
            this.options = options;
            this.hostFrameworkProvider = hostFrameworkProvider;
            this.hostTypesProvider = hostTypesProvider;
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
            var pluginAssemblyName = Path.GetFileNameWithoutExtension(pluginLoadContext.PluginAssemblyName);
            var loadContext = new NetworkAssemblyLoadContext<T>(
               logger,
               options,
               hostFrameworkProvider,
               hostTypesProvider,
               remoteTypesProvider,
               dependencyPathProvider,
               probingPathsProvider,
               runtimePlatformContext,
               depsFileProvider,
               pluginDependencyResolver,
               nativeAssemblyUnloader,
               assemblyLoadStrategyProvider,
               httpClientFactory,
               tempPathProvider
           );

            this.loadContexts[pluginAssemblyName] = loadContext;
            this.loadContextReferences[pluginAssemblyName] = new System.WeakReference(loadContext);

            return loadContext.LoadPluginAssembly(pluginLoadContext);
        }

        public virtual Task<Assembly> LoadAsync(IPluginLoadContext pluginLoadContext)
        {
            var pluginAssemblyName = Path.GetFileNameWithoutExtension(pluginLoadContext.PluginAssemblyName);
            var loadContext = new NetworkAssemblyLoadContext<T>(
               logger,
               options,
               hostFrameworkProvider,
               hostTypesProvider,
               remoteTypesProvider,
               dependencyPathProvider,
               probingPathsProvider,
               runtimePlatformContext,
               depsFileProvider,
               pluginDependencyResolver,
               nativeAssemblyUnloader,
               assemblyLoadStrategyProvider,
               httpClientFactory,
               tempPathProvider
           );

            this.loadContexts[pluginAssemblyName] = loadContext;
            this.loadContextReferences[pluginAssemblyName] = new System.WeakReference(loadContext);

            return loadContext.LoadPluginAssemblyAsync(pluginLoadContext);
        }
    }
}
