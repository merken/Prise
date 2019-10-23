using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Prise.Infrastructure.NetCore
{
    public class PluginProxy<T> : DispatchProxy
    {
        private IParameterConverter parameterConverter;
        private IResultConverter resultConverter;
        private object remoteObject;

        protected override object Invoke(MethodInfo callingMethod, object[] args)
        {
            try
            {
                var targetMethod = FindMethodOnRemoteObject(callingMethod, remoteObject);
                if (targetMethod == null)
                    throw new NotSupportedException($"Target method {callingMethod.Name} is not found on Plugin Type {remoteObject.GetType().Name}.");

                var result = targetMethod.Invoke(remoteObject, SerializeParameters(targetMethod, args));


                var remoteType = targetMethod.ReturnType;

                if (remoteType.BaseType == typeof(System.Threading.Tasks.Task))
                {
                    return this.resultConverter.ConvertToLocalTypeAsync(remoteType, result as System.Threading.Tasks.Task);
                }
                return this.resultConverter.ConvertToLocalType(remoteType, result);
            }
            catch (Exception ex) when (ex is TargetInvocationException)
            {
                throw ex.InnerException ?? ex;
            }
        }

        public static object Create() => Create<T, PluginProxy<T>>();

        internal PluginProxy<T> SetRemoteObject(object remoteObject)
        {
            if (remoteObject == null)
                throw new NotSupportedException($"Remote object for Proxy<{typeof(T).Name}> was null");

            this.remoteObject = remoteObject;
            return this;
        }

        internal PluginProxy<T> SetParameterConverter(IParameterConverter parameterConverter)
        {
            if (parameterConverter == null)
                throw new NotSupportedException($"IParameterConverter for Proxy<{typeof(T).Name}> was null");

            this.parameterConverter = parameterConverter;
            return this;
        }

        internal PluginProxy<T> SetResultConverter(IResultConverter resultConverter)
        {
            if (resultConverter == null)
                throw new NotSupportedException($"IResultConverter for Proxy<{typeof(T).Name}> was null");

            this.resultConverter = resultConverter;
            return this;
        }

        private MethodInfo FindMethodOnRemoteObject(MethodInfo callingMethod, object targetObject)
        {
            bool isNameCorrect(MethodInfo targetMethod) => targetMethod.Name == callingMethod.Name;

            var targetMethods = targetObject.GetType().GetMethods().Where(targetMethod => targetMethod.Name == callingMethod.Name);
            if (targetMethods.Count() == 0)
                throw new NotSupportedException($"Target method {callingMethod.Name} is not found on Plugin Type {targetObject.GetType().Name}.");

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
                throw new NotSupportedException($"Target method {callingMethod.Name} is found multiple times on Plugin Type {targetObject.GetType().Name}.");

            return targetMethods.First();
        }

        private object[] SerializeParameters(MethodInfo targetMethod, object[] args)
        {
            var parameters = targetMethod.GetParameters();
            var results = new List<object>();

            for (var index = 0; index < parameters.Count(); index++)
            {
                var parameter = parameters[index];
                var parameterValue = args[index];
                results.Add(this.parameterConverter.ConvertToRemoteType(parameter.ParameterType, parameterValue));
            }

            return results.ToArray();
        }
    }
}