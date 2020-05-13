using System.Reflection;
using Prise.Proxy;

namespace Prise.PluginBridge
{
    public static class PrisePluginBridge
    {
        public static object Invoke(object remoteObject, MethodInfo targetMethod, params object[] args)
            => PriseProxy.Invoke(remoteObject, targetMethod, args, new JsonSerializerParameterConverter(), new JsonSerializerResultConverter());
    }
}
