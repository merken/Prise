using Contract;
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
        internal readonly IRootPathProvider RootPathProvider;
        internal readonly IHostTypesProvider HostTypesProvider;
        internal readonly IRuntimePlatformContext RuntimePlatformContext;
        internal readonly IHostFrameworkProvider HostFrameworkProvider;
        internal readonly IDependencyPathProvider DependencyPathProvider;
        internal readonly IProbingPathsProvider ProbingPathsProvider;
        internal readonly IDepsFileProvider DepsFileProvider;
        internal readonly IPluginDependencyResolver PluginDependencyResolver;
        internal readonly INativeAssemblyUnloader NativeAssemblyUnloader;
        internal readonly IPluginServerOptions PluginServerOptions;
        internal readonly IHttpClientFactory HttpFactory;
        internal readonly IRemoteTypesProvider RemoteTypesProvider;
        internal readonly ITempPathProvider TempPathProvider;

        public FunctionPluginLoaderOptions(
            IPluginLoadOptions<IHelloPlugin> helloPluginLoadOptions,
            IRootPathProvider rootPathProvider, 
            IHostTypesProvider hostTypesProvider,
            IRemoteTypesProvider remoteTypesProvider,
            IRuntimePlatformContext runtimePlatformContext,
            IHostFrameworkProvider hostFrameworkProvider,
            IDependencyPathProvider dependencyPathProvider,
            IProbingPathsProvider probingPathsProvider,
            IDepsFileProvider depsFileProvider,
            IPluginDependencyResolver pluginDependencyResolver,
            INativeAssemblyUnloader nativeAssemblyUnloader,
            ITempPathProvider tempPathProvider,
            IPluginServerOptions pluginServerOptions,
            IHttpClientFactory httpFactory)
        {
            HelloPluginLoadOptions = helloPluginLoadOptions;
            RootPathProvider = rootPathProvider;
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
            PluginServerOptions = pluginServerOptions;
            HttpFactory = httpFactory;
        }
    }
}
