using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Prise.Infrastructure.NetCore
{
    public class DefaultAssemblyLoaderWithNativeResolver<T> : DisposableAssemblyUnLoader, IPluginAssemblyLoader<T>
    {
        public DefaultAssemblyLoaderWithNativeResolver(
            IAssemblyLoadOptions options,
            IHostFrameworkProvider hostFrameworkProvider,
            IRootPathProvider rootPathProvider,
            ILocalAssemblyLoaderOptions loaderOptions,
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
                loaderOptions,
                hostTypesProvider,
                remoteTypesProvider,
                dependencyPathProvider,
                probingPathsProvider,
                runtimePlatformContext,
                depsFileProvider,
                pluginDependencyResolver,
                nativeAssemblyUnloader
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
