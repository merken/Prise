using Prise.Infrastructure;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise
{
    public class DefaultAssemblyLoaderWithNativeResolver<T> : DisposableAssemblyUnLoader, IPluginAssemblyLoader<T>
    {
        public DefaultAssemblyLoaderWithNativeResolver(
            ILocalAssemblyLoaderOptions options,
            IHostFrameworkProvider hostFrameworkProvider,
            IRootPathProvider rootPathProvider,
            IHostTypesProvider hostTypesProvider,
            IRemoteTypesProvider remoteTypesProvider,
            IDependencyPathProvider dependencyPathProvider,
            IProbingPathsProvider probingPathsProvider,
            IRuntimePlatformContext runtimePlatformContext,
            IDepsFileProvider depsFileProvider,
            IPluginDependencyResolver pluginDependencyResolver,
            INativeAssemblyUnloader nativeAssemblyUnloader)
        {
            this.loadContext = new DefaultAssemblyLoadContextWithNativeResolver(
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
                options.PluginPath
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
