using System;
using Prise.Plugin;
using Prise.Proxy;

namespace Prise.Activation
{
    public class DefaultPluginProxyCreator : IPluginProxyCreator
    {
        public IPluginBootstrapper CreateBootstrapperProxy(object remoteBootstrapper) =>
            ProxyCreator.CreateProxy<IPluginBootstrapper>(remoteBootstrapper);

        public T CreatePluginProxy<T>(object remoteObject, IParameterConverter parameterConverter, IResultConverter resultConverter) =>
            ProxyCreator.CreateProxy<T>(remoteObject, parameterConverter, resultConverter);

        protected bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                // Nothing to do here               
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