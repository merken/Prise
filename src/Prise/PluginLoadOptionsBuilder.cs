using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Prise.AssemblyScanning;
using Prise.Infrastructure;
using Prise.Proxy;

namespace Prise
{
    public class PluginLoadOptionsBuilder<T>
    {
        internal IPluginLogger<T> logger;
        internal Type loggerType;
        internal ServiceLifetime priseServiceLifetime;
        internal CacheOptions<IPluginCache<T>> cacheOptions;
        internal IAssemblyScanner<T> assemblyScanner;
        internal Type assemblyScannerType;
        internal IAssemblyScannerOptions<T> assemblyScannerOptions;
        internal Type assemblyScannerOptionsType;
        internal IPluginPathProvider<T> pluginPathProvider;
        internal Type pluginPathProviderType;
        internal IAssemblyLoadStrategyProvider assemblyLoadStrategyProvider;
        internal Type assemblyLoadStrategyProviderType = typeof(DefaultAssemblyLoadStrategyProvider);
        internal ISharedServicesProvider<T> sharedServicesProvider;
        internal Type sharedServicesProviderType;
        internal IRemotePluginActivator activator;
        internal IPluginActivationContextProvider<T> pluginActivationContextProvider;
        internal Type pluginActivationContextProviderType;
        internal IPluginTypesProvider<T> pluginTypesProvider;
        internal Type pluginTypesProviderType;
        internal Type activatorType;
        internal IPluginProxyCreator<T> proxyCreator;
        internal Type proxyCreatorType;
        internal IResultConverter resultConverter;
        internal Type resultConverterType;
        internal IParameterConverter parameterConverter;
        internal Type parameterConverterType;
        internal IPluginAssemblyLoader<T> assemblyLoader;
        internal Type assemblyLoaderType;
        internal IPluginAssemblyNameProvider<T> pluginAssemblyNameProvider;
        internal Type pluginAssemblyNameProviderType;
        internal IAssemblyLoadOptions<T> assemblyLoadOptions;
        internal Type assemblyLoadOptionsType;
        internal INetworkAssemblyLoaderOptions<T> networkAssemblyLoaderOptions;
        internal Type networkAssemblyLoaderOptionsType;
        internal Action<IServiceCollection> configureServices;
        internal IHostTypesProvider hostTypesProvider;
        internal Type hostTypesProviderType;
        internal IRemoteTypesProvider<T> remoteTypesProvider;
        internal Type remoteTypesProviderType;
        internal IDependencyPathProvider<T> dependencyPathProvider;
        internal Type dependencyPathProviderType;
        internal IProbingPathsProvider<T> probingPathsProvider;
        internal Type probingPathsProviderType;
        internal IRuntimePlatformContext runtimePlatformContext;
        internal Type runtimePlatformContextType;
        internal IAssemblySelector<T> assemblySelector;
        internal Type assemblySelectorType;
        internal IPluginSelector<T> pluginSelector;
        internal Type pluginSelectorType;
        internal IDepsFileProvider<T> depsFileProvider;
        internal Type depsFileProviderType;
        internal IPluginDependencyResolver<T> pluginDependencyResolver;
        internal Type pluginDependencyResolverType;
        internal ITempPathProvider<T> tempPathProvider;
        internal Type tempPathProviderType;
        internal INativeAssemblyUnloader nativeAssemblyUnloader;
        internal Type nativeAssemblyUnloaderType;
        internal IHostFrameworkProvider hostFrameworkProvider;
        internal Type hostFrameworkProviderType;

        internal PluginLoadOptionsBuilder()
        {
        }

        public PluginLoadOptionsBuilder<T> WithLogger(IPluginLogger<T> logger)
        {
            this.logger = logger;
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithLoggerType<TType>()
            where TType : IPluginLogger<T>
        {
            this.loggerType = typeof(TType);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithPluginPath(string path)
        {
            this.pluginPathProvider = new DefaultPluginPathProvider<T>(path);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithPriseServiceLifetime(ServiceLifetime serviceLifetime)
        {
            this.priseServiceLifetime = serviceLifetime;
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithPluginPathProvider<TType>()
            where TType : IPluginPathProvider<T>
        {
            this.pluginPathProviderType = typeof(TType);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithSingletonCache()
        {
            this.cacheOptions = CacheOptions<IPluginCache<T>>.SingletonPluginCache();
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithCachingOptions(ServiceLifetime serviceLifetime)
        {
            this.cacheOptions = new CacheOptions<IPluginCache<T>>(serviceLifetime);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithAssemblyLoadStrategyProvider(IAssemblyLoadStrategyProvider provider)
        {
            this.assemblyLoadStrategyProvider = provider;
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithAssemblyLoadStrategyProvider<TType>()
            where TType : IAssemblyLoadStrategyProvider
        {
            this.assemblyLoadStrategyProviderType = typeof(TType);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithProxyCreator(IPluginProxyCreator<T> proxyCreator)
        {
            this.proxyCreator = proxyCreator;
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithProxyCreator<TType>()
            where TType : IPluginProxyCreator<T>
        {
            this.proxyCreatorType = typeof(TType);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithPluginAssemblyName(string pluginAssemblyName)
        {
            this.pluginAssemblyNameProvider = new PluginAssemblyNameProvider<T>(pluginAssemblyName);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithPluginAssemblyNameProvider<TType>()
                   where TType : IPluginAssemblyNameProvider<T>
        {
            this.pluginAssemblyNameProviderType = typeof(TType);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithActivator(IRemotePluginActivator activator)
        {
            this.activator = activator;
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithActivator<TType>()
            where TType : IRemotePluginActivator
        {
            this.activatorType = typeof(TType);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithParameterConverter(IParameterConverter parameterConverter)
        {
            this.parameterConverter = parameterConverter;
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithParameterConverter<TType>()
            where TType : IParameterConverter
        {
            this.parameterConverterType = typeof(TType);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithResultConverter(IResultConverter resultConverter)
        {
            this.resultConverter = resultConverter;
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithResultConverter<TType>()
            where TType : IResultConverter
        {
            this.resultConverterType = typeof(TType);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithAssemblyLoader(IPluginAssemblyLoader<T> assemblyLoader)
        {
            this.assemblyLoader = assemblyLoader;
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithAssemblyLoader<TType>()
           where TType : IPluginAssemblyLoader<T>
        {
            this.assemblyLoaderType = typeof(TType);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithLocalDiskAssemblyLoader(
            PluginPlatformVersion pluginPlatformVersion = null,
            NativeDependencyLoadPreference nativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferInstalledRuntime
            )
        {
            if (pluginPlatformVersion == null)
                pluginPlatformVersion = PluginPlatformVersion.Empty();

            this.assemblyLoadOptions = new DefaultAssemblyLoadOptions<T>(
                pluginPlatformVersion,
                false,
                nativeDependencyLoadPreference);

#if NETCORE3_0
            return this.WithAssemblyLoader<DefaultAssemblyLoaderWithNativeResolver<T>>();
#endif
#if NETCORE2_1
            return this.WithAssemblyLoader<DefaultAssemblyLoader<T>>();
#endif
        }

        public PluginLoadOptionsBuilder<T> WithLocalDiskAssemblyLoader<TType>()
            where TType : IAssemblyLoadOptions<T>
        {
            this.assemblyLoadOptionsType = typeof(TType);

#if NETCORE3_0
            return this.WithAssemblyLoader<DefaultAssemblyLoaderWithNativeResolver<T>>();
#endif
#if NETCORE2_1
            return this.WithAssemblyLoader<DefaultAssemblyLoader<T>>();
#endif
        }

        public PluginLoadOptionsBuilder<T> WithNetworkAssemblyLoader(
            string baseUrl,
            PluginPlatformVersion pluginPlatformVersion = null,
            NativeDependencyLoadPreference nativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferInstalledRuntime)
        {
            if (pluginPlatformVersion == null)
                pluginPlatformVersion = PluginPlatformVersion.Empty();

            this.networkAssemblyLoaderOptions = new DefaultNetworkAssemblyLoaderOptions<T>(
                baseUrl,
                pluginPlatformVersion,
                false,
                nativeDependencyLoadPreference);

            this.depsFileProviderType = typeof(NetworkDepsFileProvider<T>);
            this.pluginDependencyResolverType = typeof(NetworkPluginDependencyResolver<T>);
            this.assemblyLoaderType = typeof(NetworkAssemblyLoader<T>);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithNetworkAssemblyLoader<TType>()
            where TType : INetworkAssemblyLoaderOptions<T>
        {
            this.networkAssemblyLoaderOptionsType = typeof(TType);
            this.depsFileProviderType = typeof(NetworkDepsFileProvider<T>);
            this.pluginDependencyResolverType = typeof(NetworkPluginDependencyResolver<T>);
            this.assemblyLoaderType = typeof(NetworkAssemblyLoader<T>);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithNetworkTempPathProvider<TType>()
           where TType : ITempPathProvider<T>
        {
            this.tempPathProviderType = typeof(TType);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithNetworkTempPathProvider(ITempPathProvider<T> tempPathProvider)
        {
            this.tempPathProvider = tempPathProvider;
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithSharedServicesProvider<TType>()
           where TType : ISharedServicesProvider<T>
        {
            this.sharedServicesProviderType = typeof(TType);
            return this;
        }

        public PluginLoadOptionsBuilder<T> ConfigureServices(Action<IServiceCollection> configureServices)
        {
            this.configureServices = configureServices;
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithHostTypeProvider<TType>()
            where TType : IHostTypesProvider
        {
            this.hostTypesProviderType = typeof(TType);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithHostTypeProvider(IHostTypesProvider hostTypesProvider)
        {
            this.hostTypesProvider = hostTypesProvider;
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithRemoteTypesProvider<TType>()
            where TType : IRemoteTypesProvider<T>
        {
            this.remoteTypesProviderType = typeof(TType);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithRemoteTypesProvider(IRemoteTypesProvider<T> remoteTypesProvider)
        {
            this.remoteTypesProvider = remoteTypesProvider;
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithAssemblySelector(Func<IEnumerable<AssemblyScanResult<T>>, IEnumerable<AssemblyScanResult<T>>> assemblySelector)
        {
            this.assemblySelector = new DefaultAssemblySelector<T>(assemblySelector);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithAssemblySelector<TType>()
            where TType : IAssemblySelector<T>
        {
            this.assemblySelectorType = typeof(TType);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithSelector(Func<IEnumerable<Type>, IEnumerable<Type>> pluginSelector)
        {
            this.pluginSelector = new DefaultPluginSelector<T>(pluginSelector);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithSelector<TType>()
            where TType : IPluginSelector<T>
        {
            this.pluginSelectorType = typeof(TType);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithHostType(Type type)
        {
            var hostTypesProvider = this.hostTypesProvider as HostTypesProvider;
            if (hostTypesProvider == null)
                throw new PrisePluginException($"You're not using the default IHostTypesProvider {nameof(HostTypesProvider)}. Please add host types using your own provider.");
            hostTypesProvider.AddHostType(type);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithRemoteType(Type type)
        {
            var remoteTypesProvider = this.remoteTypesProvider as RemoteTypesProvider<T>;
            if (remoteTypesProvider == null)
                throw new PrisePluginException($"You're not using the default IRemoteTypesProvider {nameof(RemoteTypesProvider<T>)}. Please add remote types using your own provider.");
            remoteTypesProvider.AddRemoteType(type);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithDependencyPathProvider<TType>()
            where TType : IDependencyPathProvider<T>
        {
            this.dependencyPathProviderType = typeof(TType);
            return this;
        }

        public PluginLoadOptionsBuilder<T> UsePluginContextAsDependencyPath()
        {
            this.dependencyPathProviderType = typeof(PluginContextAsDependencyPathProvider<T>);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithProbingPath(string path)
        {
            var probingPathsProvider = this.probingPathsProvider as ProbingPathsProvider<T>;
            if (probingPathsProvider == null)
                throw new PrisePluginException($"You're not using the default IProbingPathsProvider {nameof(ProbingPathsProvider<T>)}. Please add probing paths using your own provider.");
            probingPathsProvider.AddProbingPath(path);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithProbingPathsProvider<TType>()
           where TType : IProbingPathsProvider<T>
        {
            this.probingPathsProviderType = typeof(TType);
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithProbingPathsProvider(IProbingPathsProvider<T> probingPathsProvider)
        {
            this.probingPathsProvider = probingPathsProvider;
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithDependencyPathProvider(IDependencyPathProvider<T> dependencyPathProvider)
        {
            this.dependencyPathProvider = dependencyPathProvider;
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithHostFrameworkProvider(IHostFrameworkProvider hostFrameworkProvider)
        {
            this.hostFrameworkProviderType = null;
            this.hostFrameworkProvider = hostFrameworkProvider;
            return this;
        }

        public PluginLoadOptionsBuilder<T> WithHostFrameworkProvider<TType>()
           where TType : IHostFrameworkProvider
        {
            this.hostFrameworkProviderType = typeof(TType);
            return this;
        }

        public PluginLoadOptionsBuilder<T> IgnorePlatformInconsistencies(bool ignore = true)
        {
            if (this.assemblyLoadOptionsType != null || this.networkAssemblyLoaderOptionsType != null || this.assemblyLoader != null)
                throw new PrisePluginException("Custom loaders and custom load options are not supported with IgnorePlatformInconsistencies(), please provide your own value for IgnorePlatformInconsistencies.");

            if (this.assemblyLoadOptions != null)
                this.assemblyLoadOptions = new DefaultAssemblyLoadOptions<T>(
                    this.assemblyLoadOptions.PluginPlatformVersion,
                    ignore,
                    this.assemblyLoadOptions.NativeDependencyLoadPreference);

            if (this.networkAssemblyLoaderOptions != null)
                this.networkAssemblyLoaderOptions = new DefaultNetworkAssemblyLoaderOptions<T>(
                   this.networkAssemblyLoaderOptions.BaseUrl,
                   this.networkAssemblyLoaderOptions.PluginPlatformVersion,
                   ignore,
                   this.networkAssemblyLoaderOptions.NativeDependencyLoadPreference);

            return this;
        }

        public PluginLoadOptionsBuilder<T> WithHostType<THostType>() => WithHostType(typeof(THostType));

        public PluginLoadOptionsBuilder<T> WithRemoteType<TRemoteType>() => WithRemoteType(typeof(TRemoteType));

        private IServiceCollection hostServices = new ServiceCollection();
        private IServiceCollection sharedServices = new ServiceCollection();
        public PluginLoadOptionsBuilder<T> ConfigureHostServices(Action<IServiceCollection> hostServicesConfig)
        {
            if (this.sharedServicesProviderType != null)
                throw new PrisePluginException($"A custom {typeof(ISharedServicesProvider<T>).Name} type cannot be used in combination with {nameof(ConfigureSharedServices)}service");

            this.hostServices = new ServiceCollection();
            hostServicesConfig.Invoke(this.hostServices);

            foreach (var hostService in this.hostServices)
                this
                    // A host type will always live inside the host
                    .WithHostType(hostService.ServiceType)
                    // The implementation type will always exist on the Host, since it will be created here
                    .WithHostType(hostService.ImplementationType ?? hostService.ImplementationInstance?.GetType() ?? hostService.ImplementationFactory?.Method.ReturnType)
                ;

            this.sharedServicesProvider = new DefaultSharedServicesProvider<T>(this.hostServices, this.sharedServices);
            this.activator = new DefaultRemotePluginActivator<T>(this.sharedServicesProvider);
            return this;
        }

        public PluginLoadOptionsBuilder<T> ConfigureSharedServices(Action<IServiceCollection> sharedServicesConfig)
        {
            if (this.sharedServicesProviderType != null)
                throw new PrisePluginException($"A custom {typeof(ISharedServicesProvider<T>).Name} type cannot be used in combination with {nameof(ConfigureSharedServices)}service");

            this.sharedServices = new ServiceCollection();
            sharedServicesConfig.Invoke(this.sharedServices);

            foreach (var sharedService in this.sharedServices)
                this
                    // The service type must exist on the remote to support backwards compatability
                    .WithRemoteType(sharedService.ServiceType)
                    // The implementation type will always exist on the Host, since it will be created here
                    .WithHostType(sharedService.ImplementationType ?? sharedService.ImplementationInstance?.GetType() ?? sharedService.ImplementationFactory?.Method.ReturnType)
                ; // If a shared service is added, it must be a added as a host type

            this.sharedServicesProvider = new DefaultSharedServicesProvider<T>(this.hostServices, this.sharedServices);
            this.activator = new DefaultRemotePluginActivator<T>(this.sharedServicesProvider);
            return this;
        }

        public PluginLoadOptionsBuilder<T> ScanForAssemblies(Action<AssemblyScanningComposer<T>> composerOptions)
        {
            var composer = new AssemblyScanningComposer<T>();
            composerOptions(composer.WithDefaultOptions<DefaultAssemblyScanner<T>, DefaultAssemblyScannerOptions<T>>());
            var composition = composer.Compose();

            this.assemblyScanner = composition.Scanner;
            this.assemblyScannerType = composition.ScannerType;
            this.assemblyScannerOptions = composition.ScannerOptions;
            this.assemblyScannerOptionsType = composition.ScannerOptionsType;

            return this;
        }

        public PluginLoadOptionsBuilder<T> WithDefaultOptions(string pluginPath = null, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            if (String.IsNullOrEmpty(pluginPath))
                pluginPath = Path.Join(GetLocalExecutionPath(), "Plugins");

            this.priseServiceLifetime = ServiceLifetime.Scoped;
            this.loggerType = typeof(ConsolePluginLogger<T>);
            this.pluginPathProvider = new DefaultPluginPathProvider<T>(pluginPath);
            this.dependencyPathProvider = new DependencyPathProvider<T>(pluginPath);
            this.cacheOptions = CacheOptions<IPluginCache<T>>.ScopedPluginCache();

            this.runtimePlatformContext = new RuntimePlatformContext();
            this.ScanForAssemblies(composer =>
                composer.WithDefaultOptions<DefaultAssemblyScanner<T>, DefaultAssemblyScannerOptions<T>>());

            this.pluginAssemblyNameProvider = new PluginAssemblyNameProvider<T>($"{typeof(T).Name}.dll");
            this.sharedServicesProvider = new DefaultSharedServicesProvider<T>(this.hostServices, this.sharedServices);
            this.pluginActivationContextProvider = new DefaultPluginActivationContextProvider<T>();
            this.pluginTypesProvider = new DefaultPluginTypesProvider<T>();
            this.activator = new DefaultRemotePluginActivator<T>(this.sharedServicesProvider);
            this.proxyCreator = new PluginProxyCreator<T>();

            // Use System.Text.Json in 3.0
#if NETCORE3_0
            this.parameterConverter = new JsonSerializerParameterConverter();
            this.resultConverter = new JsonSerializerResultConverter();
            this.assemblyLoaderType = typeof(DefaultAssemblyLoaderWithNativeResolver<T>);
#endif
            // Use Newtonsoft.Json in 2.1
#if NETCORE2_1
            this.parameterConverter = new NewtonsoftParameterConverter();
            this.resultConverter = new NewtonsoftResultConverter();
            this.assemblyLoaderType = typeof(DefaultAssemblyLoader<T>);
#endif
            this.assemblySelector = new DefaultAssemblySelector<T>();
            this.assemblyLoadOptions = new DefaultAssemblyLoadOptions<T>(
                PluginPlatformVersion.Empty(),
                false,
                NativeDependencyLoadPreference.PreferInstalledRuntime);

            this.probingPathsProvider = new ProbingPathsProvider<T>();

            var hostTypesProvider = new HostTypesProvider();
            hostTypesProvider.AddHostType(typeof(Prise.Plugin.PluginAttribute)); // Add the Prise.Infrastructure assembly to the host types
            hostTypesProvider.AddHostType(typeof(ServiceCollection));  // Adds the BuildServiceProvider assembly to the host types
            this.hostTypesProvider = hostTypesProvider;

            var remoteTypesProvider = new RemoteTypesProvider<T>();
            remoteTypesProvider.AddRemoteType(typeof(T)); // Add the contract to the remote types, so that we can have backwards compatibility
            this.remoteTypesProvider = remoteTypesProvider;

            this.pluginSelector = new DefaultPluginSelector<T>();
            this.depsFileProviderType = typeof(DefaultDepsFileProvider<T>);
            this.pluginDependencyResolverType = typeof(DefaultPluginDependencyResolver<T>);
            // Typically used for downloading and storing plugins from the network, but it could be useful for caching local plugins as well
            this.tempPathProviderType = typeof(UserProfileTempPathProvider<T>);

            this.nativeAssemblyUnloaderType = typeof(DefaultNativeAssemblyUnloader);
            this.hostFrameworkProviderType = typeof(HostFrameworkProvider);

            return this;
        }

        internal IServiceCollection RegisterOptions(IServiceCollection services)
        {
            // Caching
            services.Add(new ServiceDescriptor(typeof(IPluginCache<T>), typeof(DefaultScopedPluginCache<T>), this.cacheOptions.Lifetime));

            services
                // Plugin-specific services
                .RegisterTypeOrInstance<IPluginLogger<T>>(loggerType, logger, this.priseServiceLifetime)
                .RegisterTypeOrInstance<IPluginPathProvider<T>>(pluginPathProviderType, pluginPathProvider, this.priseServiceLifetime)
                .RegisterTypeOrInstance<IAssemblyScanner<T>>(assemblyScannerType, assemblyScanner, this.priseServiceLifetime)
                .RegisterTypeOrInstance<IAssemblyScannerOptions<T>>(assemblyScannerOptionsType, assemblyScannerOptions, this.priseServiceLifetime)
                .RegisterTypeOrInstance<IPluginTypesProvider<T>>(pluginTypesProviderType, pluginTypesProvider, this.priseServiceLifetime)
                .RegisterTypeOrInstance<IPluginActivationContextProvider<T>>(pluginActivationContextProviderType, pluginActivationContextProvider, this.priseServiceLifetime)
                .RegisterTypeOrInstance<IPluginProxyCreator<T>>(proxyCreatorType, proxyCreator, this.priseServiceLifetime)
                .RegisterTypeOrInstance<ISharedServicesProvider<T>>(sharedServicesProviderType, sharedServicesProvider, this.priseServiceLifetime)
                .RegisterTypeOrInstance<IPluginAssemblyNameProvider<T>>(pluginAssemblyNameProviderType, pluginAssemblyNameProvider, this.priseServiceLifetime)
                .RegisterTypeOrInstance<IPluginAssemblyLoader<T>>(assemblyLoaderType, assemblyLoader, this.priseServiceLifetime)
                .RegisterTypeOrInstance<IRemoteTypesProvider<T>>(remoteTypesProviderType, remoteTypesProvider, this.priseServiceLifetime)
                .RegisterTypeOrInstance<IDependencyPathProvider<T>>(dependencyPathProviderType, dependencyPathProvider, this.priseServiceLifetime)
                .RegisterTypeOrInstance<IProbingPathsProvider<T>>(probingPathsProviderType, probingPathsProvider, this.priseServiceLifetime)
                .RegisterTypeOrInstance<IAssemblySelector<T>>(assemblySelectorType, assemblySelector, this.priseServiceLifetime)
                .RegisterTypeOrInstance<IPluginSelector<T>>(pluginSelectorType, pluginSelector, this.priseServiceLifetime)
                .RegisterTypeOrInstance<IDepsFileProvider<T>>(depsFileProviderType, depsFileProvider, this.priseServiceLifetime)
                .RegisterTypeOrInstance<IPluginDependencyResolver<T>>(pluginDependencyResolverType, pluginDependencyResolver, this.priseServiceLifetime)
                .RegisterTypeOrInstance<ITempPathProvider<T>>(tempPathProviderType, tempPathProvider, this.priseServiceLifetime)

                // Global services
                .RegisterTypeOrInstance<IAssemblyLoadStrategyProvider>(assemblyLoadStrategyProviderType, assemblyLoadStrategyProvider, this.priseServiceLifetime)
                .RegisterTypeOrInstance<IRemotePluginActivator>(activatorType, activator, this.priseServiceLifetime)
                .RegisterTypeOrInstance<IResultConverter>(resultConverterType, resultConverter, this.priseServiceLifetime)
                .RegisterTypeOrInstance<IParameterConverter>(parameterConverterType, parameterConverter, this.priseServiceLifetime)
                .RegisterTypeOrInstance<IHostTypesProvider>(hostTypesProviderType, hostTypesProvider, this.priseServiceLifetime)
                .RegisterTypeOrInstance<IRuntimePlatformContext>(runtimePlatformContextType, runtimePlatformContext, this.priseServiceLifetime)
                .RegisterTypeOrInstance<INativeAssemblyUnloader>(nativeAssemblyUnloaderType, nativeAssemblyUnloader, this.priseServiceLifetime)
                .RegisterTypeOrInstance<IHostFrameworkProvider>(hostFrameworkProviderType, hostFrameworkProvider, this.priseServiceLifetime)
                ;

            if (assemblyLoadOptions != null)
                services
                    .Add(new ServiceDescriptor(typeof(IAssemblyLoadOptions<T>), s => assemblyLoadOptions, this.priseServiceLifetime));
            if (assemblyLoadOptionsType != null)
                services
                    .Add(new ServiceDescriptor(typeof(IAssemblyLoadOptions<T>), assemblyLoadOptionsType, this.priseServiceLifetime));

            if (networkAssemblyLoaderOptions != null)
            {
                services.Add(new ServiceDescriptor(typeof(INetworkAssemblyLoaderOptions<T>), s => networkAssemblyLoaderOptions, this.priseServiceLifetime));
                services.Add(new ServiceDescriptor(typeof(IAssemblyLoadOptions<T>), s => networkAssemblyLoaderOptions, this.priseServiceLifetime));
            }
            if (networkAssemblyLoaderOptionsType != null)
            {
                services.Add(new ServiceDescriptor(typeof(INetworkAssemblyLoaderOptions<T>), networkAssemblyLoaderOptionsType, this.priseServiceLifetime));
                services.Add(new ServiceDescriptor(typeof(IAssemblyLoadOptions<T>), networkAssemblyLoaderOptionsType, this.priseServiceLifetime));
            }

            configureServices?.Invoke(services);

            // Make use of DI by providing an injected instance of the registered services above
            services.Add(new ServiceDescriptor(typeof(IPluginLoadOptions<T>), typeof(PluginLoadOptions<T>), this.priseServiceLifetime));

            return services;
        }

        private string GetLocalExecutionPath() => AppDomain.CurrentDomain.BaseDirectory;
    }

    internal static class PluginLoadOptionsBuilderExtensions
    {
        internal static IServiceCollection RegisterTypeOrInstance<TType>(this IServiceCollection services, Type type, TType instance, ServiceLifetime serviceLifetime)
            where TType : class
        {
            if (type != null)
                services.Add(new ServiceDescriptor(typeof(TType), type, serviceLifetime));
            else if (instance != null)
                services.Add(new ServiceDescriptor(typeof(TType), s => instance, serviceLifetime));
            else
                throw new PrisePluginException($"Could not find type {type?.Name} or instance {typeof(TType).Name} to register");

            return services;
        }
    }
}