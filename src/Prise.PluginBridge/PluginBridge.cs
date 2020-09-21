using System.Reflection;
using System.Runtime.CompilerServices;
using Prise.Infrastructure;
using Prise.Proxy;

namespace Prise.PluginBridge
{
    public abstract class PluginBridge
    {
        protected object hostService;
        protected PluginBridge(object hostService)
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
}