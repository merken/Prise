using Prise.Plugin;
using Prise.Utils;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Prise.Activation
{
    public class DefaultPluginActivator : IPluginActivator, IDisposable
    {
        protected ConcurrentBag<IDisposable> disposables;
        protected IPluginActivationContextProvider pluginActivationContextProvider;
        protected IRemotePluginActivator remotePluginActivator;
        protected IPluginProxyCreator proxyCreator;

        public DefaultPluginActivator(
            Func<IPluginActivationContextProvider> pluginActivationContextProviderFactory,
            Func<IRemotePluginActivator> remotePluginActivatorFactory,
            Func<IPluginProxyCreator> proxyCreatorFactory)
        {
            this.disposables = new ConcurrentBag<IDisposable>();
            this.pluginActivationContextProvider = pluginActivationContextProviderFactory.ThrowIfNull(nameof(pluginActivationContextProviderFactory))();
            this.remotePluginActivator = remotePluginActivatorFactory.ThrowIfNull(nameof(remotePluginActivatorFactory))();
            this.proxyCreator = proxyCreatorFactory.ThrowIfNull(nameof(proxyCreatorFactory))();
        }

        public Task<T> ActivatePlugin<T>(IPluginActivationOptions pluginActivationOptions)
        {
            if (pluginActivationOptions.PluginAssembly == null)
                throw new ArgumentNullException($"{nameof(IPluginActivationOptions)}.{nameof(pluginActivationOptions.PluginAssembly)}");
            if (pluginActivationOptions.PluginType == null)
                throw new ArgumentNullException($"{nameof(IPluginActivationOptions)}.{nameof(pluginActivationOptions.PluginType)}");
            if (pluginActivationOptions.ParameterConverter == null)
                throw new ArgumentNullException($"{nameof(IPluginActivationOptions)}.{nameof(pluginActivationOptions.ParameterConverter)}");
            if (pluginActivationOptions.ResultConverter == null)
                throw new ArgumentNullException($"{nameof(IPluginActivationOptions)}.{nameof(pluginActivationOptions.ResultConverter)}");

            T pluginProxy = default(T);
            IPluginBootstrapper bootstrapperProxy = null;

            var pluginActivationContext = this.pluginActivationContextProvider.ProvideActivationContext(pluginActivationOptions.PluginType, pluginActivationOptions.PluginAssembly);

            if (pluginActivationContext.PluginBootstrapperType != null)
            {
                var remoteBootstrapperInstance = this.remotePluginActivator.CreateRemoteBootstrapper(pluginActivationContext, pluginActivationOptions.HostServices);

                var remoteBootstrapperProxy = this.proxyCreator.CreateBootstrapperProxy(remoteBootstrapperInstance);

                this.disposables.Add(remoteBootstrapperProxy as IDisposable);
                bootstrapperProxy = remoteBootstrapperProxy;
            }

            var remoteObject = this.remotePluginActivator.CreateRemoteInstance(
                pluginActivationContext,
                bootstrapperProxy,
                pluginActivationOptions.HostServices
            );

            pluginProxy = this.proxyCreator.CreatePluginProxy<T>(remoteObject, pluginActivationOptions.ParameterConverter, pluginActivationOptions.ResultConverter);

            this.disposables.Add(pluginProxy as IDisposable);

            return Task.FromResult(pluginProxy);
        }

        private volatile bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            remotePluginActivator?.Dispose();
            proxyCreator?.Dispose();

            foreach (IDisposable disposable in disposables)
            {
                disposable?.Dispose();
            }

            remotePluginActivator = null;
            proxyCreator = null;
            disposables = null;
        }
    }
}