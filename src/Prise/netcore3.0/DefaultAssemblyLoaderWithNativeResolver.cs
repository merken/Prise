using Prise.Infrastructure;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise
{
    public class DefaultAssemblyLoaderWithNativeResolver<T> : DisposableAssemblyUnLoader, IPluginAssemblyLoader<T>
    {
        public DefaultAssemblyLoaderWithNativeResolver(
            IAssemblyLoadOptions<T> options,
            IHostFrameworkProvider hostFrameworkProvider,
            IHostTypesProvider hostTypesProvider,
            IRemoteTypesProvider<T> remoteTypesProvider,
            IDependencyPathProvider<T> dependencyPathProvider,
            IProbingPathsProvider<T> probingPathsProvider,
            IRuntimePlatformContext runtimePlatformContext,
            IDepsFileProvider<T> depsFileProvider,
            IPluginDependencyResolver<T> pluginDependencyResolver,
            INativeAssemblyUnloader nativeAssemblyUnloader,
            IAssemblyLoadStrategyProvider assemblyLoadStrategyProvider)
        {
            this.loadContext = new DefaultAssemblyLoadContextWithNativeResolver<T>(
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
                assemblyLoadStrategyProvider
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
