using System;
using Prise.AssemblyScanning;
using Prise.Infrastructure;
using Prise.Proxy;

namespace Prise
{
    public class PluginLoadOptions<T> : IPluginLoadOptions<T>
    {
        private readonly IPluginLogger<T> logger;
        private readonly IAssemblyScanner<T> assemblyScanner;
        private readonly ISharedServicesProvider<T> sharedServicesProvider;
        private readonly IPluginTypesProvider<T> pluginTypesProvider;
        private readonly IPluginActivationContextProvider<T> pluginActivationContextProvider;
        private readonly IRemotePluginActivator<T> activator;
        private readonly IResultConverter resultConverter;
        private readonly IParameterConverter parameterConverter;
        private readonly IPluginAssemblyLoader<T> assemblyLoader;
        private readonly IPluginProxyCreator<T> proxyCreator;
        private readonly IHostTypesProvider<T> hostTypesProvider;
        private readonly IRemoteTypesProvider<T> remoteTypesProvider;
        private readonly IRuntimePlatformContext runtimePlatformContext;
        private readonly IAssemblySelector<T> assemblySelector;
        private readonly IPluginSelector<T> pluginSelector;
        protected bool disposed = false;

        // Use the ASP.NET Core DI system to inject these dependencies
        public PluginLoadOptions(
            IPluginLogger<T> logger,
            IAssemblyScanner<T> assemblyScanner,
            ISharedServicesProvider<T> sharedServicesProvider,
            IPluginTypesProvider<T> pluginTypesProvider,
            IPluginActivationContextProvider<T> pluginActivationContextProvider,
            IRemotePluginActivator<T> activator,
            IParameterConverter parameterConverter,
            IResultConverter resultConverter,
            IPluginAssemblyLoader<T> assemblyLoader,
            IPluginProxyCreator<T> proxyCreator,
            IHostTypesProvider<T> hostTypesProvider,
            IRemoteTypesProvider<T> remoteTypesProvider,
            IRuntimePlatformContext runtimePlatformContext,
            IAssemblySelector<T> assemblySelector,
            IPluginSelector<T> pluginSelector
            )
        {
            this.logger = logger;
            this.assemblyScanner = assemblyScanner;
            this.sharedServicesProvider = sharedServicesProvider;
            this.pluginTypesProvider = pluginTypesProvider;
            this.pluginActivationContextProvider = pluginActivationContextProvider;
            this.activator = activator;
            this.parameterConverter = parameterConverter;
            this.resultConverter = resultConverter;
            this.assemblyLoader = assemblyLoader;
            this.proxyCreator = proxyCreator;
            this.hostTypesProvider = hostTypesProvider;
            this.remoteTypesProvider = remoteTypesProvider;
            this.runtimePlatformContext = runtimePlatformContext;
            this.assemblySelector = assemblySelector;
            this.pluginSelector = pluginSelector;
        }

        public IPluginLogger<T> Logger => this.logger;
        public IAssemblyScanner<T> AssemblyScanner => this.assemblyScanner;
        public ISharedServicesProvider<T> SharedServicesProvider => this.sharedServicesProvider;
        public IPluginTypesProvider<T> PluginTypesProvider => this.pluginTypesProvider;
        public IPluginActivationContextProvider<T> PluginActivationContextProvider => this.pluginActivationContextProvider;
        public IRemotePluginActivator<T> Activator => this.activator;
        public IResultConverter ResultConverter => this.resultConverter;
        public IParameterConverter ParameterConverter => this.parameterConverter;
        public IPluginAssemblyLoader<T> AssemblyLoader => this.assemblyLoader;
        public IPluginProxyCreator<T> ProxyCreator => this.proxyCreator;
        public IHostTypesProvider<T> HostTypesProvider => this.hostTypesProvider;
        public IRemoteTypesProvider<T> RemoteTypesProvider => this.remoteTypesProvider;
        public IRuntimePlatformContext RuntimePlatformContext => this.runtimePlatformContext;
        public IAssemblySelector<T> AssemblySelector => this.assemblySelector;
        public IPluginSelector<T> PluginSelector => this.pluginSelector;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                // Dispose all properties
                this.sharedServicesProvider.Dispose();
                this.activator.Dispose();
                this.parameterConverter.Dispose();
                this.resultConverter.Dispose();
                // Unloads the loaded plugin assemblies
                this.proxyCreator.Dispose();
                this.hostTypesProvider.Dispose();
                this.remoteTypesProvider.Dispose();
                this.assemblyLoader.Dispose();
                this.assemblyScanner.Dispose();
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