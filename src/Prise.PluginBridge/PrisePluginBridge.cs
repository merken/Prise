using System.Reflection;
using Prise.Proxy;

namespace Prise.PluginBridge
{
    public static class PrisePluginBridge
    {
#if NETCORE3_0
        public static object Invoke(object remoteObject, MethodInfo targetMethod, params object[] args)
            => PriseProxy.Invoke(remoteObject, targetMethod, args, new JsonSerializerParameterConverter(), new JsonSerializerResultConverter());
#endif
#if NETCORE2_1
        public static object Invoke(object remoteObject, MethodInfo targetMethod, params object[] args)
            => PriseProxy.Invoke(remoteObject, targetMethod, args, new NewtonsoftParameterConverter(), new NewtonsoftResultConverter());
#endif
    }
}
