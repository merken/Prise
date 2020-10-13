using System.Reflection;
using System.Runtime.CompilerServices;
using Prise.Infrastructure;

namespace Prise.Proxy
{
    public abstract class ReverseProxy
    {
        protected object hostService;
        protected ReverseProxy(object hostService)
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