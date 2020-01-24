using System;
using System.Reflection;

namespace Prise.Proxy
{
    public static class ProxyCreator
    {
        public static object CreateGenericProxy(Type proxyType, object remoteObject)
        {
            return typeof(ProxyCreator).GetMethod(nameof(ProxyCreator.CreateProxy)).MakeGenericMethod(proxyType).Invoke(null, new[] { remoteObject, null, null });
        }

        public static TProxyType CreateProxy<TProxyType>(
            object remoteObject,
            IParameterConverter parameterConverter = null,
            IResultConverter resultConverter = null)
        {
            if (parameterConverter == null)
                parameterConverter = new PassthroughParameterConverter();

            if (resultConverter == null)
                resultConverter = new PassthroughResultConverter();

            var proxy = PriseProxy<TProxyType>.Create();
            ((PriseProxy<TProxyType>)proxy)
                .SetRemoteObject(remoteObject)
                .SetParameterConverter(parameterConverter)
                .SetResultConverter(resultConverter);

            return (TProxyType)proxy;
        }
    }
}
