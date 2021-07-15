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

        public void Dispose()
        {
        }
    }
}