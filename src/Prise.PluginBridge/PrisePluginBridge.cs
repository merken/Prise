using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Prise.Proxy;

namespace Prise.PluginBridge
{
    public abstract class PluginBridgeBase
    {
        protected object hostService;
        protected PluginBridgeBase(object hostService)
        {
            this.hostService = hostService;
        }

        protected T InvokeThisMethodOnHostService<T>(object[] parameters = null, [CallerMemberName] string caller = null)
            where T : class
        {
            var methodInfo = this.GetType().GetMethod(caller);
            return PriseProxy.Invoke(this.hostService, methodInfo, parameters, new JsonSerializerParameterConverter(), new JsonSerializerResultConverter()) as T;
        }
    }

    public static class PrisePluginBridge
    {
        public static object Invoke(object remoteObject, MethodInfo targetMethod, params object[] args)
            => PriseProxy.Invoke(remoteObject, targetMethod, args, new JsonSerializerParameterConverter(), new JsonSerializerResultConverter());
    }
}
