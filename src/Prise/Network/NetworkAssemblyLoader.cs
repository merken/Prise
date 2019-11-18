using Prise.Infrastructure;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise
{
    public class NetworkAssemblyLoader<T> : DisposableAssemblyUnLoader, IPluginAssemblyLoader<T>
    {
        public NetworkAssemblyLoader(
            IRootPathProvider rootPathProvider,
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
            ITempPathProvider tempPathProvider,
            IHttpClientFactory httpClientFactory
            )
        {
            this.loadContext = new NetworkAssemblyLoadContext(
                hostFrameworkProvider,
                rootPathProvider,
                options,
                hostTypesProvider,
                remoteTypesProvider,
                dependencyPathProvider,
                probingPathsProvider,
                runtimePlatformContext,
                depsFileProvider,
                pluginDependencyResolver,
                nativeAssemblyUnloader,
                httpClientFactory,
                tempPathProvider
            );
        }

        public virtual Assembly Load(string pluginAssemblyName)
        {
            return this.loadContext.LoadPluginAssembly(pluginAssemblyName);
        }

        public virtual Task<Assembly> LoadAsync(string pluginAssemblyName)
        {
            return this.loadContext.LoadPluginAssemblyAsync(pluginAssemblyName);
        }
    }
}
