using Prise.Infrastructure;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise
{
    public class DefaultAssemblyLoader<T> : DisposableAssemblyUnLoader, IPluginAssemblyLoader<T>
    {
        public DefaultAssemblyLoader(
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
            this.loadContext = new DefaultAssemblyLoadContext(
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
                options.PluginPath,
                options.IgnorePlatformInconsistencies
            );
            this.assemblyLoadContextReference = new System.WeakReference(this.loadContext);
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
