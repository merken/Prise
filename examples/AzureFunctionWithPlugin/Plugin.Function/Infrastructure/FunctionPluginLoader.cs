using System;
using System.Linq;
using System.Threading.Tasks;
using Contract;
using Prise;
using Prise.Infrastructure;

namespace Plugin.Function.Infrastructure
{
    public class FunctionPluginLoader : PluginLoader, IPluginLoader<IHelloPlugin>, IPluginResolver<IHelloPlugin>
    {
        private bool disposed = false;
        private string componentName;
        private readonly FunctionPluginLoaderOptions functionPluginLoaderOptions;

        public FunctionPluginLoader(FunctionPluginLoaderOptions functionPluginLoaderOptions)
        {
            this.functionPluginLoaderOptions = functionPluginLoaderOptions;
        }

        internal void SetComponentToLoad(string componentName)
        {
            this.componentName = componentName;
        }

        private PluginLoadOptions<IHelloPlugin> GetOptions()
        {
            var networkAssemblyLoaderOptions = new NetworkAssemblyLoaderOptions<IHelloPlugin>(
                                $"{this.functionPluginLoaderOptions.PluginServerOptions.PluginServerUrl}/{this.componentName}",
                                ignorePlatformInconsistencies: true); // The plugins are netstandard, so we must ignore inconsistencies

            var depsFileProvider = new NetworkDepsFileProvider<IHelloPlugin>(networkAssemblyLoaderOptions, this.functionPluginLoaderOptions.HttpFactory);

            var networkAssemblyLoader = new NetworkAssemblyLoader<IHelloPlugin>(
                    networkAssemblyLoaderOptions,
                    this.functionPluginLoaderOptions.HostFrameworkProvider,
                    this.functionPluginLoaderOptions.HostTypesProvider,
                    this.functionPluginLoaderOptions.RemoteTypesProvider,
                    this.functionPluginLoaderOptions.DependencyPathProvider,
                    this.functionPluginLoaderOptions.ProbingPathsProvider,
                    this.functionPluginLoaderOptions.RuntimePlatformContext,
                    depsFileProvider,
                    this.functionPluginLoaderOptions.PluginDependencyResolver,
                    this.functionPluginLoaderOptions.NativeAssemblyUnloader,
                    this.functionPluginLoaderOptions.AssemblyLoadStrategyProvider,
                    this.functionPluginLoaderOptions.TempPathProvider,
                    this.functionPluginLoaderOptions.HttpFactory);

            return new PluginLoadOptions<IHelloPlugin>(
                new FunctionPluginStaticAssemblyScanner($"{this.componentName}.dll"),
                this.functionPluginLoaderOptions.HelloPluginLoadOptions.SharedServicesProvider,
                this.functionPluginLoaderOptions.HelloPluginLoadOptions.Activator,
                this.functionPluginLoaderOptions.HelloPluginLoadOptions.ParameterConverter,
                this.functionPluginLoaderOptions.HelloPluginLoadOptions.ResultConverter,
                networkAssemblyLoader,
                this.functionPluginLoaderOptions.HelloPluginLoadOptions.ProxyCreator,
                this.functionPluginLoaderOptions.HelloPluginLoadOptions.HostTypesProvider,
                this.functionPluginLoaderOptions.HelloPluginLoadOptions.RemoteTypesProvider,
                this.functionPluginLoaderOptions.HelloPluginLoadOptions.RuntimePlatformContext,
                this.functionPluginLoaderOptions.HelloPluginLoadOptions.PluginSelector
            );
        }

        public virtual async Task<IHelloPlugin> Load()
        {
            return (await this.LoadPluginsOfTypeAsync<IHelloPlugin>(GetOptions())).First();
        }

        IHelloPlugin IPluginResolver<IHelloPlugin>.Load()
        {
            return this.LoadPluginsOfType<IHelloPlugin>(GetOptions()).First();
        }

        public virtual async Task<IHelloPlugin[]> LoadAll()
        {
            return (await this.LoadPluginsOfTypeAsync<IHelloPlugin>(GetOptions()));
        }

        IHelloPlugin[] IPluginResolver<IHelloPlugin>.LoadAll()
        {
            return this.LoadPluginsOfType<IHelloPlugin>(GetOptions());
        }

        public virtual async Task Unload()
        {
            await this.functionPluginLoaderOptions.HelloPluginLoadOptions.AssemblyLoader.UnloadAsync();
        }

        void IPluginResolver<IHelloPlugin>.Unload()
        {
            this.functionPluginLoaderOptions.HelloPluginLoadOptions.AssemblyLoader.Unload();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                foreach (var disposable in this.disposables)
                    disposable.Dispose();

                // Remove the lock on the loaded assemblies
                this.pluginAssemblies = null;
                // Disposes all configured services for the PluginLoadOptions
                // Including the AssemblyLoader
                this.functionPluginLoaderOptions?.HelloPluginLoadOptions?.Dispose();

            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}