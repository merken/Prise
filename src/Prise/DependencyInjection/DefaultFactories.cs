using System;
using System.Threading.Tasks;
using Prise.Activation;
using Prise.AssemblyLoading;
using Prise.AssemblyScanning;
using Prise.Core;
using Prise.Proxy;
using Prise.Infrastructure;
using Prise.Platform;
using System.Collections.Generic;

namespace Prise.DependencyInjection
{
    public static class DefaultFactories
    {
        public static Func<IRuntimeDefaultAssemblyContext> RuntimeDefaultAssemblyContext = () => new RuntimeDefaultAssemblyContext();
        public static Func<INugetPackageUtilities> DefaultNugetPackageUtilities = () => new DefaultNugetPackageUtilities();
        public static Func<IPlatformAbstraction> DefaultPlatformAbstraction = () => new DefaultPlatformAbstraction();
        public static Func<IDirectoryTraverser> DefaultDirectoryTraverser = () => new DefaultDirectoryTraverser();
        public static Func<string, IMetadataLoadContext> DefaultMetadataLoadContext = (fullPathToAssembly) => new DefaultMetadataLoadContext(fullPathToAssembly);
        public static Func<IAssemblyScanner> DefaultAssemblyScanner = () => new DefaultAssemblyScanner(DefaultMetadataLoadContext, DefaultDirectoryTraverser);
        public static Func<IAssemblyScanner> DefaultNuGetAssemblyScanner = () => new DefaultNugetPackageAssemblyScanner(DefaultMetadataLoadContext, DefaultDirectoryTraverser, DefaultNugetPackageUtilities);
        public static Func<IPluginTypeSelector> DefaultPluginTypeSelector = () => new DefaultPluginTypeSelector();
        public static Func<IParameterConverter> DefaultParameterConverter = () => new JsonSerializerParameterConverter();
        public static Func<IResultConverter> DefaultResultConverter = () => new JsonSerializerResultConverter();
        public static Func<IPluginActivator> DefaultPluginActivator = () => new DefaultPluginActivator(DefaultPluginActivationContextProvider, DefaultRemotePluginActivator, DefaultPluginProxyCreator);
        public static Func<IPluginActivationContextProvider> DefaultPluginActivationContextProvider = () => new DefaultPluginActivationContextProvider();
        public static Func<IRemotePluginActivator> DefaultRemotePluginActivator = () => new DefaultRemotePluginActivator(DefaultBootstrapperServiceProvider, DefaultPluginServiceProvider);
        public static Func<IServiceProvider, IEnumerable<Type>, IBootstrapperServiceProvider> DefaultBootstrapperServiceProvider = (sp, hostTypes) => new DefaultBootstrapperServiceProvider(sp, hostTypes);
        public static Func<IServiceProvider, IEnumerable<Type>, IEnumerable<Type>, IPluginServiceProvider> DefaultPluginServiceProvider = (sp, hostTypes, pluginTypes) => new DefaultPluginServiceProvider(sp, hostTypes, pluginTypes);
        public static Func<IPluginProxyCreator> DefaultPluginProxyCreator = () => new DefaultPluginProxyCreator();
        public static Func<IAssemblyLoader> DefaultAssemblyLoader = () => new DefaultAssemblyLoader(DefaultAssemblyLoadContextFactory);
        public static Func<INativeAssemblyUnloader> DefaultNativeAssemblyUnloaderFactory = () => new DefaultNativeAssemblyUnloader();
        public static Func<string, IAssemblyDependencyResolver> DefaultAssemblyDependencyResolver = (p) => new DefaultAssemblyDependencyResolver(p);
        public static Func<IFileSystemUtilities> DefaultFileSystemUtilities = () => new DefaultFileSystemUtilities();
        public static Func<IPluginDependencyContextProvider> DefaultPluginDependencyContextProvider = () => new DefaultPluginDependencyContextProvider(DefaultRuntimePlatformContextFactory);
        public static Func<IAssemblyLoadContext> DefaultAssemblyLoadContextFactory = () => new DefaultAssemblyLoadContext(
                    DefaultNativeAssemblyUnloaderFactory,
                    DefaultPluginDependencyResolverFactory,
                    DefaultAssemblyLoadStrategyFactory,
                    DefaultAssemblyDependencyResolver,
                    DefaultFileSystemUtilities,
                    RuntimeDefaultAssemblyContext,
                    DefaultPluginDependencyContextProvider
        );
        public static Func<IRuntimePlatformContext> DefaultRuntimePlatformContextFactory = () => new DefaultRuntimePlatformContext(DefaultPlatformAbstraction, DefaultDirectoryTraverser);
        public static Func<IAssemblyLoadStrategy> DefaultAssemblyLoadStrategyFactory = () => new DefaultAssemblyLoadStrategy();
        public static Func<IPluginDependencyResolver> DefaultPluginDependencyResolverFactory = () => new DefaultPluginDependencyResolver(DefaultRuntimePlatformContextFactory);
    }
}