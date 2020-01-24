using System;
using System.Linq;
using System.Reflection;
using Prise.Infrastructure;

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

            var factoryMethodWithPluginServiceProvider = remoteType.GetMethods()
                    .FirstOrDefault(m =>
                        m.CustomAttributes.Any(c => c.AttributeType.Name == typeof(Prise.Plugin.PluginFactoryAttribute).Name) &&
                        // This is the new PluginFactory 
                        m.GetParameters().Any(p => p.ParameterType.Name == typeof(Prise.Plugin.IPluginServiceProvider).Name));

            return new PluginActivationContext
            {
                PluginType = remoteType,
                PluginAssembly = pluginAssembly,
                PluginBootstrapperType = bootstrapper,
                PluginFactoryMethod = factoryMethod,
                PluginFactoryMethodWithPluginServiceProvider = factoryMethodWithPluginServiceProvider
            };
        }
    }
}
