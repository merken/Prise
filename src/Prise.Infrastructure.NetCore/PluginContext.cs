using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Prise.Infrastructure.NetCore
{
    public class PluginContext : IPluginContext
    {
        private readonly Type pluginType;
        private readonly Dictionary<Type, object> dependencyProvider;

        public PluginContext(Type pluginType, IServiceProvider serviceProvider)
        {
            this.pluginType = pluginType;
            this.dependencyProvider = new Dictionary<Type, object>();
            var constructor = pluginType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).First();

            foreach (var parameter in constructor.GetParameters())
            {
                var dependency = serviceProvider.GetService(parameter.ParameterType);
                if (dependency == null)
                    throw new ArgumentException($"Dependency {parameter.ParameterType} {parameter.Name} was not found for type {pluginType.Name}");

                this.dependencyProvider[parameter.ParameterType] = dependency;
            }
        }

        public Type PluginType => pluginType;
        public Dictionary<Type, object> DependencyProvider => dependencyProvider;
    }
}
