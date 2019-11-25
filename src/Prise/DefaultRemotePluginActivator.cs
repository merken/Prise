using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Prise.Infrastructure;
using Prise.Plugin;

namespace Prise
{
    public class DefaultRemotePluginActivator<T> : IRemotePluginActivator
    {
        private readonly ISharedServicesProvider<T> sharedServicesProvider;
        private bool disposed = false;

        public DefaultRemotePluginActivator(ISharedServicesProvider<T> sharedServicesProvider)
        {
            this.sharedServicesProvider = sharedServicesProvider;
        }

        public object CreateRemoteBootstrapper(Type bootstrapperType, Assembly assembly)
        {
            var contructors = bootstrapperType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var firstCtor = contructors.First();

            if (!contructors.Any())
                throw new PrisePluginException($"No public constructors found for remote bootstrapper {bootstrapperType.Name}");
            if (firstCtor.GetParameters().Any())
                throw new PrisePluginException($"Bootstrapper {bootstrapperType.Name} must contain a public parameterless constructor");

            return assembly.CreateInstance(bootstrapperType.FullName);
        }

        public object CreateRemoteInstance(Type pluginType, IPluginBootstrapper bootstrapper, MethodInfo factoryMethod, Assembly assembly)
        {
            var contructors = pluginType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            if (contructors.Count() > 1)
                throw new PrisePluginException($"Multiple public constructors found for remote plugin {pluginType.Name}");

            var firstCtor = contructors.FirstOrDefault();
            if (firstCtor != null && !firstCtor.GetParameters().Any())
                return assembly.CreateInstance(pluginType.FullName);

            if (factoryMethod == null)
                throw new PrisePluginException($@"Plugins must either provide a default parameterless constructor or implement a static factory method.
                    Like; 'public static {pluginType.Name} CreatePlugin(IServiceProvider serviceProvider)");

            var sharedServices = this.sharedServicesProvider.ProvideSharedServices();

            if (bootstrapper != null)
                sharedServices = bootstrapper.Bootstrap(sharedServices);

            var serviceProvider = sharedServices.BuildServiceProvider();

            return factoryMethod.Invoke(null, new[] { serviceProvider });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                // Nothing to do here
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