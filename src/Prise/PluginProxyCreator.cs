using System;
using Prise.Infrastructure;
using Prise.Proxy;
using Prise.Plugin;

namespace Prise
{
    /// <summary>
    /// This proxy creator uses the Prise.Proxy package to create a proxy for the plugin
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PluginProxyCreator<T> :  IPluginProxyCreator<T>
    {
        protected bool disposed = false;

        public IPluginBootstrapper CreateBootstrapperProxy(object remoteBootstrapper) =>
            ProxyCreator.CreateProxy<IPluginBootstrapper>(remoteBootstrapper);

        public T CreatePluginProxy(object remoteObject, IParameterConverter parameterConverter, IResultConverter resultConverter) =>
            ProxyCreator.CreateProxy<T>(remoteObject, parameterConverter, resultConverter);

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