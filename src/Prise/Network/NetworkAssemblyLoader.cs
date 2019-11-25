using Prise.Infrastructure;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise
{
    public class NetworkAssemblyLoader<T> : DisposableAssemblyUnLoader, IPluginAssemblyLoader<T>
    {
        public NetworkAssemblyLoader(
            INetworkAssemblyLoaderOptions options,
            IHostFrameworkProvider hostFrameworkProvider,
            IHostTypesProvider hostTypesProvider,
            IRemoteTypesProvider remoteTypesProvider,
            IDependencyPathProvider dependencyPathProvider,
            IProbingPathsProvider probingPathsProvider,
            IRuntimePlatformContext runtimePlatformContext,
            IDepsFileProvider depsFileProvider,
            IPluginDependencyResolver pluginDependencyResolver,
            INativeAssemblyUnloader nativeAssemblyUnloader,
            IAssemblyLoadStrategyProvider assemblyLoadStrategyProvider,
            ITempPathProvider tempPathProvider,
            IHttpClientFactory httpClientFactory
            )
        {
            this.loadContext = new NetworkAssemblyLoadContext(
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
