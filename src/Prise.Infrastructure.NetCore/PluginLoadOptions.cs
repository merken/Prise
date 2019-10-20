using System;
using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Prise.Infrastructure.NetCore.Contracts;

namespace Prise.Infrastructure.NetCore
{
    public interface IPluginLoadOptions<T>
    {
        IRootPathProvider RootPathProvider { get; }
        ISharedServicesProvider SharedServicesProvider { get; }
        IRemotePluginActivator Activator { get; }
        IResultConverter ResultConverter { get; }
        IParameterConverter ParameterConverter { get; }
        IPluginAssemblyLoader<T> AssemblyLoader { get; }
        IPluginAssemblyNameProvider PluginAssemblyNameProvider { get; }
    }

    public class PluginLoadOptions<T> : IPluginLoadOptions<T>
    {
        private readonly IRootPathProvider rootPathProvider;
        private readonly ISharedServicesProvider sharedServicesProvider;
        private readonly IRemotePluginActivator activator;
        private readonly IResultConverter resultConverter;
        private readonly IParameterConverter parameterConverter;
        private readonly IPluginAssemblyLoader<T> assemblyLoader;
        private readonly IPluginAssemblyNameProvider pluginAssemblyNameProvider;

        public PluginLoadOptions(
            IRootPathProvider rootPathProvider,
            ISharedServicesProvider sharedServicesProvider,
            IRemotePluginActivator activator,
            IParameterConverter parameterConverter,
            IResultConverter resultConverter,
            IPluginAssemblyLoader<T> assemblyLoader,
            IPluginAssemblyNameProvider pluginAssemblyNameProvider)
        {
            this.rootPathProvider = rootPathProvider;
            this.sharedServicesProvider = sharedServicesProvider;
            this.activator = activator;
            this.parameterConverter = parameterConverter;
            this.resultConverter = resultConverter;
            this.assemblyLoader = assemblyLoader;
            this.pluginAssemblyNameProvider = pluginAssemblyNameProvider;
        }

        public IRootPathProvider RootPathProvider => this.rootPathProvider;
        public ISharedServicesProvider SharedServicesProvider => this.sharedServicesProvider;
        public IRemotePluginActivator Activator => this.activator;
        public IResultConverter ResultConverter => this.resultConverter;
        public IParameterConverter ParameterConverter => this.parameterConverter;
        public IPluginAssemblyLoader<T> AssemblyLoader => this.assemblyLoader;
        public IPluginAssemblyNameProvider PluginAssemblyNameProvider => this.pluginAssemblyNameProvider;
    }

    public class PluggerOptionsBuilder<T>
    {
        internal IRootPathProvider rootPathProvider;
        internal Type rootPathProviderType;
        internal ISharedServicesProvider sharedServicesProvider;
        internal Type sharedServicesProviderType;
        internal IRemotePluginActivator activator;
        internal Type activatorType;
        internal IResultConverter resultConverter;
        internal Type resultConverterType;
        internal IParameterConverter parameterConverter;
        internal Type parameterConverterType;
        internal IPluginAssemblyLoader<T> assemblyLoader;
        internal Type assemblyLoaderType;
        internal IPluginAssemblyNameProvider pluginAssemblyNameProvider;
        internal Type pluginAssemblyNameProviderType;
        internal ILocalAssemblyLoaderOptions localAssemblyLoaderOptions;
        internal Type localAssemblyLoaderOptionsType;
        internal INetworkAssemblyLoaderOptions networkAssemblyLoaderOptions;
        internal Type networkAssemblyLoaderOptionsType;
        internal bool supportMultiplePlugins;
        internal Action<IServiceCollection> configureServices;

        internal PluggerOptionsBuilder()
        {
        }

        public PluggerOptionsBuilder<T> WithRootPath(string path)
        {
            this.rootPathProvider = new RootPathProvider(path);
            return this;
        }

        public PluggerOptionsBuilder<T> WithRootPathProvider<TType>()
            where TType : IRootPathProvider
        {
            this.rootPathProviderType = typeof(TType);
            return this;
        }

        public PluggerOptionsBuilder<T> WithPluginAssemblyName(string pluginAssemblyName)
        {
            this.pluginAssemblyNameProvider = new PluginAssemblyNameProvider(pluginAssemblyName);
            return this;
        }

        public PluggerOptionsBuilder<T> WithPluginAssemblyNameProvider<TType>()
                   where TType : IPluginAssemblyNameProvider
        {
            this.pluginAssemblyNameProviderType = typeof(TType);
            return this;
        }

        public PluggerOptionsBuilder<T> WithActivator(IRemotePluginActivator activator)
        {
            this.activator = activator;
            return this;
        }

        public PluggerOptionsBuilder<T> WithActivator<TType>()
            where TType : IRemotePluginActivator
        {
            this.activatorType = typeof(TType);
            return this;
        }

        public PluggerOptionsBuilder<T> WithParameterConverter(IParameterConverter parameterConverter)
        {
            this.parameterConverter = parameterConverter;
            return this;
        }

        public PluggerOptionsBuilder<T> WithParameterConverter<TType>()
            where TType : IParameterConverter
        {
            this.parameterConverterType = typeof(TType);
            return this;
        }

        public PluggerOptionsBuilder<T> WithResultConverter(IResultConverter resultConverter)
        {
            this.resultConverter = resultConverter;
            return this;
        }

        public PluggerOptionsBuilder<T> WithResultConverter<TType>()
            where TType : IResultConverter
        {
            this.resultConverterType = typeof(TType);
            return this;
        }

        public PluggerOptionsBuilder<T> WithAssemblyLoader(IPluginAssemblyLoader<T> assemblyLoader)
        {
            this.assemblyLoader = assemblyLoader;
            return this;
        }

        public PluggerOptionsBuilder<T> WithAssemblyLoader<TType>()
           where TType : IPluginAssemblyLoader<T>
        {
            this.assemblyLoaderType = typeof(TType);
            return this;
        }

        public PluggerOptionsBuilder<T> WithLocalDiskAssemblyLoader(string pluginPath)
        {
            this.localAssemblyLoaderOptions = new LocalAssemblyLoaderOptions(pluginPath);
            return this.WithAssemblyLoader<LocalDiskAssemblyLoader<T>>();
        }

        public PluggerOptionsBuilder<T> WithLocalDiskAssemblyLoader<TType>()
            where TType : ILocalAssemblyLoaderOptions
        {
            this.localAssemblyLoaderOptionsType = typeof(TType);
            return this.WithAssemblyLoader<LocalDiskAssemblyLoader<T>>();
        }

        public PluggerOptionsBuilder<T> WithNetworkAssemblyLoader(string baseUrl)
        {
            this.networkAssemblyLoaderOptions = new NetworkAssemblyLoaderOptions(baseUrl);
            return this.WithAssemblyLoader<NetworkAssemblyLoader<T>>();
        }

        public PluggerOptionsBuilder<T> WithNetworkAssemblyLoader<TType>()
            where TType : INetworkAssemblyLoaderOptions
        {
            this.networkAssemblyLoaderOptionsType = typeof(TType);
            return this.WithAssemblyLoader<NetworkAssemblyLoader<T>>();
        }

        public PluggerOptionsBuilder<T> WithSharedServicesProvider<TType>()
           where TType : ISharedServicesProvider
        {
            this.sharedServicesProviderType = typeof(TType);
            return this;
        }

        public PluggerOptionsBuilder<T> SupportMultiplePlugins(bool supportMultiplePlugins = true)
        {
            this.supportMultiplePlugins = supportMultiplePlugins;
            return this;
        }

        public PluggerOptionsBuilder<T> ConfigureServices(Action<IServiceCollection> configureServices)
        {
            this.configureServices = configureServices;
            return this;
        }

        public PluggerOptionsBuilder<T> ConfigureSharedServices(Action<IServiceCollection> sharedServices)
        {
            if (this.sharedServicesProviderType != null)
                throw new NotSupportedException($"A custom {typeof(ISharedServicesProvider).Name} type cannot be used in combination with {nameof(ConfigureSharedServices)}service");

            var services = new ServiceCollection();
            sharedServices.Invoke(services);
            this.sharedServicesProvider = new DefaultSharedServicesProvider(services);
            this.activator = new NetCoreActivator(this.sharedServicesProvider);
            return this;
        }

        public PluggerOptionsBuilder<T> WithDefaultOptions(string rootPath = null)
        {
            if (String.IsNullOrEmpty(rootPath))
                rootPath = GetLocalExecutionPath();

            this.rootPathProvider = new RootPathProvider(rootPath);
            this.sharedServicesProvider = new DefaultSharedServicesProvider(new ServiceCollection());
            this.activator = new NetCoreActivator(this.sharedServicesProvider);
            this.parameterConverter = new NewtonsoftParameterConverter();
            this.resultConverter = new BinaryFormatterResultConverter();
            this.assemblyLoader = new LocalDiskAssemblyLoader<T>(this.rootPathProvider, new LocalAssemblyLoaderOptions("Plugins"));
            this.pluginAssemblyNameProvider = new PluginAssemblyNameProvider($"{typeof(T).Name}.dll");
            this.supportMultiplePlugins = false;

            return this;
        }

        internal IServiceCollection RegisterOptions(IServiceCollection services)
        {
            services
                .RegisterTypeOrInstance<IRootPathProvider>(rootPathProviderType, rootPathProvider)
                .RegisterTypeOrInstance<ISharedServicesProvider>(sharedServicesProviderType, sharedServicesProvider)
                .RegisterTypeOrInstance<IPluginAssemblyNameProvider>(pluginAssemblyNameProviderType, pluginAssemblyNameProvider)
                .RegisterTypeOrInstance<IRemotePluginActivator>(activatorType, activator)
                .RegisterTypeOrInstance<IResultConverter>(resultConverterType, resultConverter)
                .RegisterTypeOrInstance<IParameterConverter>(parameterConverterType, parameterConverter)
                .RegisterTypeOrInstance<IPluginAssemblyLoader<T>>(assemblyLoaderType, assemblyLoader);

            if (localAssemblyLoaderOptions != null)
                services.AddScoped<ILocalAssemblyLoaderOptions>(s => localAssemblyLoaderOptions);
            if (localAssemblyLoaderOptionsType != null)
                services.AddScoped(typeof(ILocalAssemblyLoaderOptions), localAssemblyLoaderOptionsType);

            if (networkAssemblyLoaderOptions != null)
                services.AddScoped<INetworkAssemblyLoaderOptions>(s => networkAssemblyLoaderOptions);
            if (networkAssemblyLoaderOptionsType != null)
                services.AddScoped(typeof(INetworkAssemblyLoaderOptions), networkAssemblyLoaderOptionsType);

            configureServices?.Invoke(services);

            // Make use of DI by providing an injected instance of the registered services above
            return services.AddScoped<IPluginLoadOptions<T>, PluginLoadOptions<T>>();
        }

        private string GetLocalExecutionPath()
        {
            var localExecutionPath = Assembly.GetExecutingAssembly().Location;
            var paths = localExecutionPath.Split("\\");
            return String.Join("\\", paths, 0, paths.Length - 1);
        }
    }

    internal static class PluginLoadOptionsBuilderExtensions
    {
        internal static IServiceCollection RegisterTypeOrInstance<TType>(this IServiceCollection services, Type type, TType instance)
            where TType : class
        {
            if (type != null)
                services.AddScoped(typeof(TType), type);
            else if (instance != null)
                services.AddScoped<TType>(s => instance);
            else
                throw new NotSupportedException($"Could not find type {type.Name} or instance {typeof(TType).Name} to register");

            return services;
        }
    }
}