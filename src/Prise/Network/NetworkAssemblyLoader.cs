using Prise.Infrastructure;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise
{
    public class NetworkAssemblyLoader<T> : DisposableAssemblyUnLoader, IPluginAssemblyLoader<T>
    {
        public NetworkAssemblyLoader(
            INetworkAssemblyLoaderOptions<T> options,
            IHostFrameworkProvider hostFrameworkProvider,
            IHostTypesProvider hostTypesProvider,
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
            this.loadContext = new NetworkAssemblyLoadContext<T>(
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
            this.assemblyLoadContextReference = new System.WeakReference(this.loadContext);
        }

        public virtual Assembly Load(IPluginLoadContext pluginLoadContext)
        {
            return this.loadContext.LoadPluginAssembly(pluginLoadContext);
        }

        public virtual Task<Assembly> LoadAsync(IPluginLoadContext pluginLoadContext)
        {
            return this.loadContext.LoadPluginAssemblyAsync(pluginLoadContext);
        }
    }
}
