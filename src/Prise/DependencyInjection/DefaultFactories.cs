using System;
using System.Threading.Tasks;
using Prise.Activation;
using Prise.AssemblyLoading;
using Prise.AssemblyScanning;
using Prise.Core;
using Prise.Proxy;
using Prise.Infrastructure;
using Prise.Platform;

namespace Prise.DependencyInjection
{
    public static class DefaultFactories
    {
        public static Func<IAssemblyScanner> DefaultAssemblyScanner = () => new DefaultAssemblyScanner();
        public static Func<IAssemblyScanner> DefaultNuGetAssemblyScanner = () => new DefaultNugetPackageAssemblyScanner();
        public static Func<IPluginTypeSelector> DefaultPluginTypeSelector = () => new DefaultPluginTypeSelector();
        public static Func<IParameterConverter> DefaultParameterConverter = () => new JsonSerializerParameterConverter();
        public static Func<IResultConverter> DefaultResultConverter = () => new JsonSerializerResultConverter( );
        public static Func<IPluginActivator> DefaultPluginActivator = () => new DefaultPluginActivator(DefaultPluginActivationContextProvider, DefaultRemotePluginActivator,DefaultPluginProxyCreator );
        public static Func<IPluginActivationContextProvider> DefaultPluginActivationContextProvider = () => new DefaultPluginActivationContextProvider();
        public static Func<IRemotePluginActivator> DefaultRemotePluginActivator = () => new DefaultRemotePluginActivator();
        public static Func<IPluginProxyCreator> DefaultPluginProxyCreator = () => new DefaultPluginProxyCreator();
        public static Func<IAssemblyLoader> DefaultAssemblyLoader = () => new DefaultAssemblyLoader(DefaultAssemblyLoadContextFactory);
        public static Func<INativeAssemblyUnloader> DefaultNativeAssemblyUnloaderFactory = () => new DefaultNativeAssemblyUnloader();
        public static Func<IAssemblyLoadContext> DefaultAssemblyLoadContextFactory = () => new DefaultAssemblyLoadContext(
                    DefaultNativeAssemblyUnloaderFactory,
                    DefaultPluginDependencyResolverFactory,
                    DefaultAssemblyLoadStrategyFactory,
                    DefaultPluginDependencyContextFactory
        );
        public static Func<IRuntimePlatformContext> DefaultRuntimePlatformContextFactory = () => new DefaultRuntimePlatformContext();
        public static Func<IAssemblyLoadStrategy> DefaultAssemblyLoadStrategyFactory = () => new DefaultAssemblyLoadStrategy();
        public static Func<IPluginDependencyResolver> DefaultPluginDependencyResolverFactory = () => new DefaultPluginDependencyResolver(DefaultRuntimePlatformContextFactory);
        public static Func<IPluginLoadContext, Task<IPluginDependencyContext>> DefaultPluginDependencyContextFactory = (pluginLoadContext) => DefaultPluginDependencyContext.FromPluginLoadContext(pluginLoadContext);
    }
}