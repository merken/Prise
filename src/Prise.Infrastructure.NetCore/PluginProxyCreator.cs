using System;

namespace Prise.Infrastructure.NetCore
{
    public class PluginProxyCreator<T> : IProxyCreator<T>
    {
        protected bool disposed = false;

        public IPluginBootstrapper CreateBootstrapperProxy(object remoteBootstrapper)
        {
            var proxy = PluginProxy<IPluginBootstrapper>.Create();
            ((PluginProxy<IPluginBootstrapper>)proxy)
                .SetRemoteObject(remoteBootstrapper)
                .SetParameterConverter(new PassthroughParameterConverter())
                .SetResultConverter(new PassthroughResultConverter());
            return (IPluginBootstrapper)proxy;
        }

        public T CreatePluginProxy(object remoteObject, IPluginLoadOptions<T> options)
        {
            var proxy = PluginProxy<T>.Create();
            ((PluginProxy<T>)proxy)
                .SetRemoteObject(remoteObject)
                .SetParameterConverter(options.ParameterConverter)
                .SetResultConverter(options.ResultConverter);
            return (T)proxy;
        }

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