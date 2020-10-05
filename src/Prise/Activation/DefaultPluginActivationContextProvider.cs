using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Prise.Plugin;

namespace Prise.Activation
{
    public class DefaultPluginActivationContextProvider : IPluginActivationContextProvider
    {
        public IPluginActivationContext ProvideActivationContext(Type remoteType, IAssemblyShim pluginAssembly)
        {
            var bootstrapper = pluginAssembly
                    .Types
                    .FirstOrDefault(t => t.CustomAttributes
                        .Any(c => c.AttributeType.Name == typeof(Prise.Plugin.PluginBootstrapperAttribute).Name &&
                        (c.NamedArguments.First(a => a.MemberName == "PluginType").TypedValue.Value as Type).Name == remoteType.Name));

            // TODO End support for PluginFactories by next major release
            var factoryMethod = remoteType.GetMethods()
                    .FirstOrDefault(m => m.CustomAttributes
                        .Any(c => c.AttributeType.Name == typeof(Prise.Plugin.PluginFactoryAttribute).Name));

            var pluginServices = new List<PluginService>();
            var typeInfo = remoteType as System.Reflection.TypeInfo;

            var pluginServiceFields = GetFieldsOfCustomAttribute(typeInfo, typeof(Prise.Plugin.PluginServiceAttribute).Name);
            if (pluginServiceFields.Any())
                pluginServices = pluginServiceFields.Select(f => ParsePluginService(remoteType, f)).ToList();

            var bootstrapperServices = new List<BootstrapperService>();
            if (bootstrapper != null)
            {
                var bootstrapperServiceFields = GetFieldsOfCustomAttribute(bootstrapper as System.Reflection.TypeInfo, typeof(Prise.Plugin.BootstrapperServiceAttribute).Name);
                if (bootstrapperServiceFields.Any())
                    bootstrapperServices = bootstrapperServiceFields.Select(f => ParseBootstrapperService(bootstrapper, f)).ToList();
            }

            var pluginActivatedMethod = GetPluginActivatedMethod(remoteType);

            return new DefaultPluginActivationContext
            {
                PluginType = remoteType,
                PluginAssembly = pluginAssembly,
                PluginBootstrapperType = bootstrapper,
                PluginFactoryMethod = factoryMethod,
                PluginActivatedMethod = pluginActivatedMethod,
                PluginServices = pluginServices,
                BootstrapperServices = bootstrapperServices,
            };
        }

        private static MethodInfo GetPluginActivatedMethod(Type type)
        {
            var pluginActivatedMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(m => m.CustomAttributes.Any(c => c.AttributeType.Name == typeof(Prise.Plugin.PluginActivatedAttribute).Name));

            if (pluginActivatedMethods.Count() > 1)
                throw new PluginActivationException($"Type {type.Name} contains multiple PluginActivated methods, please provide only one private void OnActivated(){{}} method and annotate it with the PluginActivated Attribute.");

            return pluginActivatedMethods.FirstOrDefault();
        }

        private static IEnumerable<FieldInfo> GetFieldsOfCustomAttribute(TypeInfo type, string typeName)
        {
            return type.DeclaredFields
                    .Where(f => f.CustomAttributes.Any(c => c.AttributeType.Name == typeName));
        }

        private static PluginService ParsePluginService(Type remoteType, FieldInfo field)
        {
            var attribute = field.CustomAttributes.First(c => c.AttributeType.Name == typeof(Prise.Plugin.PluginServiceAttribute).Name);
            var serviceTypeArg = attribute.NamedArguments.FirstOrDefault(a => a.MemberName == "ServiceType");
            if (serviceTypeArg == null)
                throw new PluginActivationException($"The ServiceType {remoteType.Name} argument is required for the {nameof(PluginServiceAttribute)}.");

            var serviceType = serviceTypeArg.TypedValue.Value as Type;

            ProvidedBy providedBy = default(ProvidedBy);
            var providedByArg = attribute.NamedArguments.FirstOrDefault(a => a.MemberName == "ProvidedBy");
            if (providedByArg != null)
            {
                var providedByArgValue = providedByArg.TypedValue.Value;
                if (providedByArgValue != null)
                    providedBy = (ProvidedBy)(int)providedByArgValue;
            }

            Type proxyType = GetProxyTypeFromAttribute(attribute);

            return new PluginService
            {
                ProvidedBy = providedBy,
                ServiceType = serviceType,
                FieldName = field.Name,
                ProxyType = proxyType
            };
        }

        private static BootstrapperService ParseBootstrapperService(Type remoteType, FieldInfo field)
        {
            var attribute = field.CustomAttributes.First(c => c.AttributeType.Name == typeof(Prise.Plugin.BootstrapperServiceAttribute).Name);
            var serviceTypeArg = attribute.NamedArguments.FirstOrDefault(a => a.MemberName == "ServiceType");
            if (serviceTypeArg == null)
                throw new PluginActivationException($"The ServiceType {remoteType.Name} argument is required for the {nameof(BootstrapperServiceAttribute)}.");

            var serviceType = serviceTypeArg.TypedValue.Value as Type;

            Type proxyType = GetProxyTypeFromAttribute(attribute);

            return new BootstrapperService
            {
                ServiceType = serviceType,
                FieldName = field.Name,
                ProxyType = proxyType
            };
        }

        private static Type GetProxyTypeFromAttribute(CustomAttributeData attribute)
        {
            Type proxyType = null;
            var proxyTypeArg = attribute.NamedArguments.FirstOrDefault(a => a.MemberName == "ProxyType");
            if (proxyTypeArg.TypedValue.Value == null) // TODO End support of BridgeType by next major release
                proxyTypeArg = attribute.NamedArguments.FirstOrDefault(a => a.MemberName == "BridgeType");

            if (proxyTypeArg.TypedValue.Value != null)
                proxyType = proxyTypeArg.TypedValue.Value as Type;

            return proxyType;
        }
    }
}