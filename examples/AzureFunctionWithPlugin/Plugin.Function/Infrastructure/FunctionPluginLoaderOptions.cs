using Contract;
using Prise;
using Prise.Infrastructure;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Plugin.Function.Infrastructure
{
    public class FunctionPluginLoaderOptions
    {
        internal readonly IPluginLoadOptions<IHelloPlugin> HelloPluginLoadOptions;
        internal readonly IPluginPathProvider<IHelloPlugin> PluginPathProvider;
        internal readonly IHostTypesProvider HostTypesProvider;
        internal readonly IRuntimePlatformContext RuntimePlatformContext;
        internal readonly IHostFrameworkProvider HostFrameworkProvider;
        internal readonly IDependencyPathProvider<IHelloPlugin> DependencyPathProvider;
        internal readonly IProbingPathsProvider<IHelloPlugin> ProbingPathsProvider;
        internal readonly IDepsFileProvider<IHelloPlugin> DepsFileProvider;
        internal readonly IPluginDependencyResolver<IHelloPlugin> PluginDependencyResolver;
        internal readonly INativeAssemblyUnloader NativeAssemblyUnloader;
        internal readonly IRemoteTypesProvider<IHelloPlugin> RemoteTypesProvider;
        internal readonly ITempPathProvider<IHelloPlugin> TempPathProvider;
        internal readonly IAssemblyLoadStrategyProvider AssemblyLoadStrategyProvider;
        internal readonly IPluginServerOptions PluginServerOptions;
        internal readonly IHttpClientFactory HttpFactory;

        public FunctionPluginLoaderOptions(
            IPluginLoadOptions<IHelloPlugin> helloPluginLoadOptions,
            IPluginPathProvider<IHelloPlugin> pluginPathProvider, 
            IHostTypesProvider hostTypesProvider,
            IRemoteTypesProvider<IHelloPlugin> remoteTypesProvider,
            IRuntimePlatformContext runtimePlatformContext,
            IHostFrameworkProvider hostFrameworkProvider,
            IDependencyPathProvider<IHelloPlugin> dependencyPathProvider,
            IProbingPathsProvider<IHelloPlugin> probingPathsProvider,
            IDepsFileProvider<IHelloPlugin> depsFileProvider,
            IPluginDependencyResolver<IHelloPlugin> pluginDependencyResolver,
            INativeAssemblyUnloader nativeAssemblyUnloader,
            ITempPathProvider<IHelloPlugin> tempPathProvider,
            IAssemblyLoadStrategyProvider assemblyLoadStrategyProvider,
            IPluginServerOptions pluginServerOptions,
            IHttpClientFactory httpFactory)
        {
            HelloPluginLoadOptions = helloPluginLoadOptions;
            PluginPathProvider = pluginPathProvider;
            HostTypesProvider = hostTypesProvider;
            RemoteTypesProvider = remoteTypesProvider;
            RuntimePlatformContext = runtimePlatformContext;
            HostFrameworkProvider = hostFrameworkProvider;
            DependencyPathProvider = dependencyPathProvider;
            ProbingPathsProvider = probingPathsProvider;
            DepsFileProvider = depsFileProvider;
            PluginDependencyResolver = pluginDependencyResolver;
            NativeAssemblyUnloader = nativeAssemblyUnloader;
            TempPathProvider = tempPathProvider;
            AssemblyLoadStrategyProvider = assemblyLoadStrategyProvider;
            PluginServerOptions = pluginServerOptions;
            HttpFactory = httpFactory;
        }
    }
}
