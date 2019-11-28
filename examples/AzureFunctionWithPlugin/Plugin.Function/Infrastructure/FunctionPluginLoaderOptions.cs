using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Contract;
using Prise;
using Prise.Infrastructure;

namespace Plugin.Function.Infrastructure
{
    public class FunctionPluginLoaderOptions
    {
        private readonly IPluginLoadOptions<IHelloPlugin> helloPluginLoadOptions;
        private readonly IPluginPathProvider<IHelloPlugin> pluginPathProvider;
        private readonly IHostTypesProvider hostTypesProvider;
        private readonly IRuntimePlatformContext runtimePlatformContext;
        private readonly IHostFrameworkProvider hostFrameworkProvider;
        private readonly IDependencyPathProvider<IHelloPlugin> dependencyPathProvider;
        private readonly IProbingPathsProvider<IHelloPlugin> probingPathsProvider;
        private readonly IPluginDependencyResolver<IHelloPlugin> pluginDependencyResolver;
        private readonly INativeAssemblyUnloader nativeAssemblyUnloader;
        private readonly IRemoteTypesProvider<IHelloPlugin> remoteTypesProvider;
        private readonly ITempPathProvider<IHelloPlugin> tempPathProvider;
        private readonly IAssemblyLoadStrategyProvider assemblyLoadStrategyProvider;
        private readonly IPluginServerOptions pluginServerOptions;
        private readonly IHttpClientFactory httpFactory;

        public FunctionPluginLoaderOptions(
            IPluginLoadOptions<IHelloPlugin> helloPluginLoadOptions,
            IPluginPathProvider<IHelloPlugin> pluginPathProvider,
            IHostTypesProvider hostTypesProvider,
            IRemoteTypesProvider<IHelloPlugin> remoteTypesProvider,
            IRuntimePlatformContext runtimePlatformContext,
            IHostFrameworkProvider hostFrameworkProvider,
            IDependencyPathProvider<IHelloPlugin> dependencyPathProvider,
            IProbingPathsProvider<IHelloPlugin> probingPathsProvider,
            IPluginDependencyResolver<IHelloPlugin> pluginDependencyResolver,
            INativeAssemblyUnloader nativeAssemblyUnloader,
            ITempPathProvider<IHelloPlugin> tempPathProvider,
            IAssemblyLoadStrategyProvider assemblyLoadStrategyProvider,
            IPluginServerOptions pluginServerOptions,
            IHttpClientFactory httpFactory)
        {
            this.helloPluginLoadOptions = helloPluginLoadOptions;
            this.pluginPathProvider = pluginPathProvider;
            this.hostTypesProvider = hostTypesProvider;
            this.remoteTypesProvider = remoteTypesProvider;
            this.runtimePlatformContext = runtimePlatformContext;
            this.hostFrameworkProvider = hostFrameworkProvider;
            this.dependencyPathProvider = dependencyPathProvider;
            this.probingPathsProvider = probingPathsProvider;
            this.pluginDependencyResolver = pluginDependencyResolver;
            this.nativeAssemblyUnloader = nativeAssemblyUnloader;
            this.tempPathProvider = tempPathProvider;
            this.assemblyLoadStrategyProvider = assemblyLoadStrategyProvider;
            this.pluginServerOptions = pluginServerOptions;
            this.httpFactory = httpFactory;
        }

        public IPluginLoader<IHelloPlugin> CreateLoaderForComponent(string functionComponent)
        {
            var networkAssemblyLoaderOptions = new NetworkAssemblyLoaderOptions<IHelloPlugin>(
                                $"{this.pluginServerOptions.PluginServerUrl}/{functionComponent}",
                                ignorePlatformInconsistencies: true); // The plugins are netstandard, so we must ignore inconsistencies

            var depsFileProvider = new NetworkDepsFileProvider<IHelloPlugin>(networkAssemblyLoaderOptions, this.httpFactory);

            var networkAssemblyLoader = new NetworkAssemblyLoader<IHelloPlugin>(
                    networkAssemblyLoaderOptions,
                    this.hostFrameworkProvider,
                    this.hostTypesProvider,
                    this.remoteTypesProvider,
                    this.dependencyPathProvider,
                    this.probingPathsProvider,
                    this.runtimePlatformContext,
                    depsFileProvider,
                    this.pluginDependencyResolver,
                    this.nativeAssemblyUnloader,
                    this.assemblyLoadStrategyProvider,
                    this.tempPathProvider,
                    this.httpFactory);

            var loaderOptions =  new PluginLoadOptions<IHelloPlugin>(
                new StaticAssemblyScanner<IHelloPlugin>($"{functionComponent}.dll", String .Empty),
                this.helloPluginLoadOptions.SharedServicesProvider,
                this.helloPluginLoadOptions.Activator,
                this.helloPluginLoadOptions.ParameterConverter,
                this.helloPluginLoadOptions.ResultConverter,
                networkAssemblyLoader,
                this.helloPluginLoadOptions.ProxyCreator,
                this.helloPluginLoadOptions.HostTypesProvider,
                this.helloPluginLoadOptions.RemoteTypesProvider,
                this.helloPluginLoadOptions.RuntimePlatformContext,
                this.helloPluginLoadOptions.PluginSelector
            );
             
            return new PrisePluginLoader<IHelloPlugin>(loaderOptions);
        }
    }
}