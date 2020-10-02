using System;
using System.Linq;
using System.Reflection;

namespace Prise
{
    public static class Testing
    {
        public static T CreateTestPluginInstance<T>(params object[] pluginServices)
        {
            var pluginType = typeof(T);
            var pluginInstance = typeof(T).Assembly.CreateInstance(typeof(T).FullName);
            var services = pluginType.GetTypeInfo().DeclaredFields.Where(f => f.CustomAttributes.Any(c => c.AttributeType.Name == typeof(Prise.Plugin.PluginServiceAttribute).Name));
            foreach (var service in services)
            {
                var serviceType = service.FieldType;
                var pluginService = pluginServices.FirstOrDefault(p => p.GetType().Name == serviceType.Name);
                if (pluginService == null)
                    throw new ArgumentException($"A pluginService of type {serviceType.Name} is required for activating plugin {pluginType.Name}.");

                pluginInstance
                    .GetType()
                    .GetTypeInfo()
                        .DeclaredFields
                            .First(f => f.Name == service.Name)
                            .SetValue(pluginInstance, pluginService);

                var activationMethod = pluginType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).FirstOrDefault(m => m.CustomAttributes.Any(c => c.AttributeType.Name == typeof(Prise.Plugin.PluginActivatedAttribute).Name));
                activationMethod.Invoke(pluginInstance, null);
            }

            return (T)pluginInstance;
        }
    }
}
