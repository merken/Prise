using Prise.Infrastructure;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise
{
    public class DefaultAssemblyLoaderWithNativeResolver<T> : DisposableAssemblyUnLoader, IPluginAssemblyLoader<T>
    {
        public DefaultAssemblyLoaderWithNativeResolver(
            IAssemblyLoadOptions options,
            IHostFrameworkProvider hostFrameworkProvider,
            IHostTypesProvider hostTypesProvider,
            IRemoteTypesProvider remoteTypesProvider,
            IDependencyPathProvider dependencyPathProvider,
            IProbingPathsProvider probingPathsProvider,
            IRuntimePlatformContext runtimePlatformContext,
            IDepsFileProvider depsFileProvider,
            IPluginDependencyResolver pluginDependencyResolver,
            INativeAssemblyUnloader nativeAssemblyUnloader,
            IAssemblyLoadStrategyProvider assemblyLoadStrategyProvider)
        {
            this.loadContext = new DefaultAssemblyLoadContextWithNativeResolver(
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
