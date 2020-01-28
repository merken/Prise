using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Prise.Infrastructure;
using Prise.Plugin;

namespace Prise
{
    public class DefaultPluginActivationContextProvider<T> : IPluginActivationContextProvider<T>
    {
        public PluginActivationContext ProvideActivationContext(Type remoteType, Assembly pluginAssembly)
        {
            var bootstrapper = pluginAssembly
                    .GetTypes()
                    .FirstOrDefault(t => t.CustomAttributes
                        .Any(c => c.AttributeType.Name == typeof(Prise.Plugin.PluginBootstrapperAttribute).Name &&
                        (c.NamedArguments.First(a => a.MemberName == "PluginType").TypedValue.Value as Type).Name == remoteType.Name));

            var factoryMethod = remoteType.GetMethods()
                    .FirstOrDefault(m => m.CustomAttributes
                        .Any(c => c.AttributeType.Name == typeof(Prise.Plugin.PluginFactoryAttribute).Name));

            var pluginServices = new List<PluginService>();
            var typeInfo = remoteType as System.Reflection.TypeInfo;

            var pluginServiceFields = GetFieldsOfCustomAttribute(typeInfo, typeof(Prise.Plugin.PluginServiceAttribute).Name);
            if (pluginServiceFields.Any())
            {
                pluginServices = pluginServiceFields.Select(f => ParsePluginService(f)).ToList();
            }

            var pluginActivatedMethod = GetPluginActivatedMethod(remoteType);

            return new PluginActivationContext
            {
                PluginType = remoteType,
                PluginAssembly = pluginAssembly,
                PluginBootstrapperType = bootstrapper,
                PluginFactoryMethod = factoryMethod,
                PluginServices = pluginServices,
                PluginActivatedMethod = pluginActivatedMethod
            };
        }

        private static MethodInfo GetPluginActivatedMethod(Type type)
        {
            var pluginActivatedMethods = type.GetMethods()
                .Where(m => m.CustomAttributes.Any(c => c.AttributeType.Name == typeof(Prise.Plugin.PluginActivatedAttribute).Name));

            if (pluginActivatedMethods.Count() > 1)
                throw new PrisePluginException($"Type {type.Name} contains multiple PluginActivated methods, please provide only one public void OnActivated(){{}} method and annotate it with the PluginActivated Attribute.");

            return pluginActivatedMethods.FirstOrDefault();
        }

        private static IEnumerable<FieldInfo> GetFieldsOfCustomAttribute(TypeInfo type, string typeName)
        {
            return type.DeclaredFields
                    .Where(f => f.CustomAttributes.Any(c => c.AttributeType.Name == typeName));
        }

        private static PluginService ParsePluginService(FieldInfo field)
        {
            var attribute = field.CustomAttributes.First(c => c.AttributeType.Name == typeof(Prise.Plugin.PluginServiceAttribute).Name);
            var serviceTypeArg = attribute.NamedArguments.FirstOrDefault(a => a.MemberName == "ServiceType");
            if (serviceTypeArg == null)
                throw new PrisePluginException($"The ServiceType {typeof(T).Name} argument is required for the PluginServiceAttribute.");

            var serviceType = serviceTypeArg.TypedValue.Value as Type;

            ProvidedBy providedBy = default(ProvidedBy);
            var providedByArg = attribute.NamedArguments.FirstOrDefault(a => a.MemberName == "ProvidedBy");
            if (providedByArg != null)
            {
                var providedByArgValue = providedByArg.TypedValue.Value;
                if (providedByArgValue != null)
                    providedBy = (ProvidedBy)(int)providedByArgValue;
            }

            Type bridgeType = null;
            var bridgeTypeArg = attribute.NamedArguments.FirstOrDefault(a => a.MemberName == "BridgeType");
            if (bridgeTypeArg != null)
            {
                var bridgeTypeArgValue = bridgeTypeArg.TypedValue.Value;
                if (bridgeTypeArgValue != null)
                    bridgeType = bridgeTypeArgValue as Type;
            }

            return new PluginService
            {
                ProvidedBy = providedBy,
                ServiceType = serviceType,
                FieldName = field.Name,
                BridgeType = bridgeType
            };
        }
    }
}
