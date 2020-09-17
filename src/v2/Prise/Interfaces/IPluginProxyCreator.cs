using System;
using Prise.Plugin;
using Prise.Proxy;

namespace Prise.V2
{
    public interface IPluginProxyCreator : IDisposable
    {
        IPluginBootstrapper CreateBootstrapperProxy(object remoteBootstrapper);
        T CreatePluginProxy<T>(object remoteObject, IParameterConverter parameterConverter, IResultConverter resultConverter);
    }
}