using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Prise.Infrastructure;

namespace Prise.Proxy
{
    /// <summary>
    /// The ReverseProxy is a base class that will provide a proxy to a Host Service from the Plugin (in reverse).
    /// </summary>
    public abstract class ReverseProxy
    {
        protected object hostService;
        protected ReverseProxy(object hostService)
        {
            this.hostService = hostService;
        }

        private MethodBase GetCallingMethod() => new StackTrace().GetFrame(2).GetMethod();

        /// <summary>
        /// This handles void proxy calls to the hostService
        /// </summary>
        /// <param name="parameters">The list of method parameters</param>
        protected void InvokeOnHostService(params object[] parameters)
        {
            var callingMethod = GetCallingMethod();
            var methodInfo = PriseProxy.FindMethodOnObject(callingMethod as MethodInfo, this);
            if (methodInfo.GetParameters().Count() != parameters.Count())
                throw new ReverseProxyException($"The number of parameters provided to this ReverseProxy {parameters?.Count()} do not match the actual parameter count of the hostService method ({methodInfo.GetParameters().Count()}). Did you forget to provide the correct number of parameters?");

            this.Invoke(hostService, methodInfo, parameters ?? new object[] { });
        }

        /// <summary>
        /// This handles proxy calls to the hostService
        /// </summary>
        /// <param name="parameters">The list of method parameters</param>
        /// <typeparam name="T">Return reference type of the calling method</typeparam>
        /// <returns>The response of the invocation on the host object</returns>
        protected T InvokeOnHostService<T>(params object[] parameters)
        {
            var callingMethod = GetCallingMethod();
            var methodInfo = PriseProxy.FindMethodOnObject(callingMethod as MethodInfo, this);
            if (methodInfo.GetParameters().Count() != parameters.Count())
                throw new ReverseProxyException($"The number of parameters provided to this ReverseProxy {parameters?.Count()} do not match the actual parameter count of the hostService method ({methodInfo.GetParameters().Count()}). Did you forget to provide the correct number of parameters?");

            return (T)this.Invoke(hostService, methodInfo, parameters ?? new object[] { });
        }

        private object Invoke(object hostService, MethodInfo methodInfo, object[] parameters) => PriseProxy.Invoke(hostService, methodInfo, parameters ?? new object[] { }, new JsonSerializerParameterConverter(), new JsonSerializerResultConverter());
    }
}