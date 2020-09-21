using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Prise.Proxy.Exceptions;

namespace Prise.Proxy
{
    /// <summary>
    /// This is the PriseProxy static class that encapsulates most of the boilerplate static methods in order to interact with the remote object (Plugin)
    /// TODO
    /// - Generic Methods (Task<string> Do<T>(T stuff))
    /// - Events (not sure if this is ever possible)
    /// </summary>
    public static class PriseProxy
    {
        public static object Invoke(object remoteObject, MethodInfo targetMethod, object[] args)
            => Invoke(remoteObject, targetMethod, args, new PassthroughParameterConverter(), new PassthroughResultConverter());

        public static object Invoke(object remoteObject, MethodInfo targetMethod, object[] args, IParameterConverter parameterConverter, IResultConverter resultConverter)
        {
            try
            {
                var localType = targetMethod.ReturnType;
                var remoteMethod = FindMethodOnRemoteObject(targetMethod, remoteObject);
                if (remoteMethod == null)
                    throw new PriseProxyException($"Target method {targetMethod.Name} is not found on Proxy Type {remoteObject.GetType().Name}.");

                object result = null;
                if (remoteMethod.IsGenericMethod)
                {
                    var generic = remoteMethod.MakeGenericMethod(targetMethod.GetGenericArguments());
                    result = generic.Invoke(remoteObject, SerializeParameters(parameterConverter, remoteMethod, args));
                }
                else
                    result = remoteMethod.Invoke(remoteObject, SerializeParameters(parameterConverter, remoteMethod, args));

                var remoteType = remoteMethod.ReturnType;
                if (remoteType.BaseType == typeof(System.Threading.Tasks.Task))
                {
                    return resultConverter.ConvertToLocalTypeAsync(localType, remoteType, result as System.Threading.Tasks.Task);
                }
                return resultConverter.ConvertToLocalType(localType, remoteType, result);
            }
            catch (Exception ex) when (ex is TargetInvocationException)
            {
                throw ex.InnerException ?? ex;
            }
        }

        internal static MethodInfo FindMethodOnRemoteObject(MethodInfo callingMethod, object targetObject)
        {
            bool isNameCorrect(MethodInfo targetMethod) => targetMethod.Name == callingMethod.Name;

            var targetMethods = targetObject.GetType().GetMethods().Where(targetMethod => targetMethod.Name == callingMethod.Name);
            if (!targetMethods.Any())
                throw new PriseProxyException($"Target method {callingMethod.Name} is not found on Proxy Type {targetObject.GetType().Name}.");

            if (targetMethods.Count() == 1)
                return targetMethods.First();

            bool isParameterCountCorrect(MethodInfo targetMethod) => targetMethod.GetParameters().Count() == callingMethod.GetParameters().Count();

            bool doAllParametersMatch(MethodInfo targetMethod)
            {
                var callingMethodParameters = callingMethod.GetParameters();
                var targetMethodParameters = targetMethod.GetParameters();
                for (var index = 0; index < callingMethodParameters.Count(); index++)
                {
                    var callingParam = callingMethodParameters[index];
                    var targetParam = targetMethodParameters[index];
                    if (!(targetParam.Name == callingParam.Name &&
                        targetParam.ParameterType.Name == callingParam.ParameterType.Name))
                        return false;
                }
                return true;
            }

            targetMethods = targetObject.GetType().GetMethods().Where(targetMethod =>
                isNameCorrect(targetMethod) &&
                isParameterCountCorrect(targetMethod) &&
                doAllParametersMatch(targetMethod)
            );

            if (targetMethods.Count() > 1)
                throw new PriseProxyException($"Target method {callingMethod.Name} is found multiple times on Proxy Type {targetObject.GetType().Name}.");

            return targetMethods.First();
        }

        internal static object[] SerializeParameters(IParameterConverter parameterConverter, MethodInfo targetMethod, object[] args)
        {
            var parameters = targetMethod.GetParameters();
            var results = new List<object>();

            for (var index = 0; index < parameters.Count(); index++)
            {
                var parameter = parameters[index];
                var parameterValue = args[index];

                if (parameter.ParameterType.BaseType == typeof(System.MulticastDelegate))
                {
                    if (parameter.ParameterType.GenericTypeArguments.Any(g => g != typeof(EventArgs)))
                        throw new PriseProxyException($"Custom EventArgs are not supported in Prise");

                    results.Add(parameterValue);
                    continue;
                }

                object result = null;
                if (parameter.ParameterType.IsGenericParameter)
                {
                    var runtimeType = parameterValue.GetType();
                    result = parameterConverter.ConvertToRemoteType(runtimeType, parameterValue);
                }
                else
                    result = parameterConverter.ConvertToRemoteType(parameter.ParameterType, parameterValue);

                results.Add(result);
            }

            return results.ToArray();
        }
    }

    /// <summary>
    /// This is the PriseProxy wrapper class that will acts as the communication layer between the Host and the Plugin.
    /// Every call from the Host to the Plugin will go through here.
    /// </summary>
    /// <typeparam name="T">The Plugin type</typeparam>
    public class PriseProxy<T> : DispatchProxy, IDisposable
    {
        private IParameterConverter parameterConverter;
        private IResultConverter resultConverter;
        private object remoteObject;
        protected bool disposed = false;

        protected override object Invoke(MethodInfo targetMethod, object[] args) => PriseProxy.Invoke(this.remoteObject, targetMethod, args, this.parameterConverter, this.resultConverter);

        public static object Create() => Create<T, PriseProxy<T>>();

        internal PriseProxy<T> SetRemoteObject(object remoteObject)
        {
            if (remoteObject == null)
                throw new PriseProxyException($"Remote object for Proxy<{typeof(T).Name}> was null");

            this.remoteObject = remoteObject;
            return this;
        }

        internal PriseProxy<T> SetParameterConverter(IParameterConverter parameterConverter)
        {
            if (parameterConverter == null)
                throw new PriseProxyException($"IParameterConverter for Proxy<{typeof(T).Name}> was null");

            this.parameterConverter = parameterConverter;
            return this;
        }

        internal PriseProxy<T> SetResultConverter(IResultConverter resultConverter)
        {
            if (resultConverter == null)
                throw new PriseProxyException($"IResultConverter for Proxy<{typeof(T).Name}> was null");

            this.resultConverter = resultConverter;
            return this;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                this.parameterConverter = null;
                this.resultConverter = null;
                this.remoteObject = null;
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
