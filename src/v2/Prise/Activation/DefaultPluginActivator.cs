using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Prise.Plugin;
using Prise.Proxy;

namespace Prise.Activation
{
    public class DefaultPluginActivator : IPluginActivator, IDisposable
    {
        private bool disposed = false;
        protected ConcurrentBag<IDisposable> disposables;

        public DefaultPluginActivator()
        {
            this.disposables = new ConcurrentBag<IDisposable>();
        }

        public Task<T> ActivatePlugin<T>(IPluginActivationContext pluginActivationContext)
        {
            parameterConverter = parameterConverter ?? new JsonSerializerParameterConverter();
            resultConverter = resultConverter ?? new JsonSerializerResultConverter();

            T pluginProxy = default(T);
            IPluginBootstrapper bootstrapperProxy = null;

            var pluginActivationContextProvider = new DefaultPluginActivationContextProvider();
            var pluginActivationContext = pluginActivationContextProvider.ProvideActivationContext(pluginType, pluginAssembly);

            var remoteActivator = new DefaultRemotePluginActivator(sharedServices, hostServices); // todo shared services
            var proxyCreator = new DefaultPluginProxyCreator();

            if (pluginActivationContext.PluginBootstrapperType != null)
            {
                var remoteBootstrapperInstance = remoteActivator.CreateRemoteBootstrapper(pluginActivationContext.PluginBootstrapperType, pluginAssembly);

                var remoteBootstrapperProxy = proxyCreator.CreateBootstrapperProxy(remoteBootstrapperInstance);

                this.disposables.Add(remoteBootstrapperProxy as IDisposable);
                bootstrapperProxy = remoteBootstrapperProxy;
            }

            var remoteObject = remoteActivator.CreateRemoteInstance(
                pluginActivationContext,
                bootstrapperProxy
            );

            pluginProxy = proxyCreator.CreatePluginProxy<T>(remoteObject, parameterConverter, resultConverter);

            this.disposables.Add(pluginProxy as IDisposable);

            return Task.FromResult(pluginProxy);
        }

         protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                foreach (var disposable in this.disposables)
                    disposable.Dispose();
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