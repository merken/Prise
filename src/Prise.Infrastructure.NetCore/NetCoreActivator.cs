using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Prise.Infrastructure.NetCore
{
    public class NetCoreActivator : IRemotePluginActivator
    {
        public object CreateRemoteBootstrapper(Type bootstrapperType)
        {
            var contructors = bootstrapperType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var firstCtor = contructors.First();

            if (contructors.Count() == 0)
                throw new NotSupportedException($"No public constructors found for remote bootstrapper {bootstrapperType.Name}");
            if (firstCtor.GetParameters().Count() > 0)
                throw new NotSupportedException($"Bootstrapper {bootstrapperType.Name} must contain a public parameterless constructor");

            return Activator.CreateInstance(bootstrapperType);

        }

        public object CreateRemoteInstance(Type pluginType, IPluginBootstrapper bootstrapper, MethodInfo factoryMethod)
        {
            var contructors = pluginType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            if (contructors.Count() > 1)
                throw new NotSupportedException($"Multiple public constructors found for remote plugin {pluginType.Name}");

            var firstCtor = contructors.FirstOrDefault();
            if (firstCtor != null && firstCtor.GetParameters().Count() == 0)
                return Activator.CreateInstance(pluginType);

            if (factoryMethod == null)
                throw new NotSupportedException($@"Plugins must either provide a default parameterless constructor or implement a static factory method.
                    Like; 'public static {pluginType.Name} CreatePlugin(IServiceProvider serviceProvider)");

            if (bootstrapper == null)
                throw new ArgumentNullException($"The type requires dependencies, please provide a {nameof(IPluginBootstrapper)} for plugin {pluginType.Name}");

            var serviceProvider = CreateServiceProviderForType(bootstrapper);
            return factoryMethod.Invoke(null, new[] { serviceProvider });
        }

        private IServiceProvider CreateServiceProviderForType(IPluginBootstrapper bootstrapper) =>
            bootstrapper.Bootstrap(new ServiceCollection()).BuildServiceProvider();
    }
}