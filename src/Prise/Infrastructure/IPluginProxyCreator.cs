using Prise.Proxy;
using Prise.Plugin;
using System;

namespace Prise.Infrastructure
{
    public interface IPluginProxyCreator<T> : IDisposable
    {
        IPluginBootstrapper CreateBootstrapperProxy(object remoteBootstrapper);
        T CreatePluginProxy(object remoteObject, IParameterConverter parameterConverter, IResultConverter resultConverter);
    }
}