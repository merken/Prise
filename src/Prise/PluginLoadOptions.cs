using System;
using Prise.AssemblyScanning;
using Prise.Infrastructure;

namespace Prise
{
    public class PluginLoadOptions<T> : IPluginLoadOptions<T>
    {
        private readonly IAssemblyScanner<T> assemblyScanner;
        private readonly ISharedServicesProvider<T> sharedServicesProvider;
        private readonly IRemotePluginActivator activator;
        private readonly IResultConverter resultConverter;
        private readonly IParameterConverter parameterConverter;
        private readonly IPluginAssemblyLoader<T> assemblyLoader;
        private readonly IProxyCreator<T> proxyCreator;
        private readonly IHostTypesProvider hostTypesProvider;
        private readonly IRemoteTypesProvider<T> remoteTypesProvider;
        private readonly IRuntimePlatformContext runtimePlatformContext;
        private readonly IPluginSelector<T> pluginSelector;
        protected bool disposed = false;

        public PluginLoadOptions(
            IAssemblyScanner<T> assemblyScanner,
            ISharedServicesProvider<T> sharedServicesProvider,
            IRemotePluginActivator activator,
            IParameterConverter parameterConverter,
            IResultConverter resultConverter,
            IPluginAssemblyLoader<T> assemblyLoader,
            IProxyCreator<T> proxyCreator,
            IHostTypesProvider hostTypesProvider,
            IRemoteTypesProvider<T> remoteTypesProvider,
            IRuntimePlatformContext runtimePlatformContext,
            IPluginSelector<T> pluginSelector
            )
        {
            this.assemblyScanner = assemblyScanner;
            this.sharedServicesProvider = sharedServicesProvider;
            this.activator = activator;
            this.parameterConverter = parameterConverter;
            this.resultConverter = resultConverter;
            this.assemblyLoader = assemblyLoader;
            this.proxyCreator = proxyCreator;
            this.hostTypesProvider = hostTypesProvider;
            this.remoteTypesProvider = remoteTypesProvider;
            this.runtimePlatformContext = runtimePlatformContext;
            this.pluginSelector = pluginSelector;
        }

        public IAssemblyScanner<T> AssemblyScanner => this.assemblyScanner;
        public ISharedServicesProvider<T> SharedServicesProvider => this.sharedServicesProvider;
        public IRemotePluginActivator Activator => this.activator;
        public IResultConverter ResultConverter => this.resultConverter;
        public IParameterConverter ParameterConverter => this.parameterConverter;
        public IPluginAssemblyLoader<T> AssemblyLoader => this.assemblyLoader;
        public IProxyCreator<T> ProxyCreator => this.proxyCreator;
        public IHostTypesProvider HostTypesProvider => this.hostTypesProvider;
        public IRemoteTypesProvider<T> RemoteTypesProvider => this.remoteTypesProvider;
        public IRuntimePlatformContext RuntimePlatformContext => this.runtimePlatformContext;
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