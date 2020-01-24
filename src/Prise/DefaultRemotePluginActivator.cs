using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Prise.Infrastructure;
using Prise.Plugin;
using Prise.Proxy;

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

        public virtual object CreateRemoteInstance(PluginActivationContext pluginActivationContext, IPluginBootstrapper bootstrapper = null)
        {
            // TODO Check out RuntimeHelpers.GetUninitializedObject(pluginType);
            var pluginType = pluginActivationContext.PluginType;
            var pluginAssembly = pluginActivationContext.PluginAssembly;
            var factoryMethod = pluginActivationContext.PluginFactoryMethod;

            var contructors = pluginType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            if (contructors.Count() > 1)
                throw new PrisePluginException($"Multiple public constructors found for remote plugin {pluginType.Name}");

            var firstCtor = contructors.FirstOrDefault();
            if (firstCtor != null && !firstCtor.GetParameters().Any())
                return pluginAssembly.CreateInstance(pluginType.FullName);

            if (factoryMethod == null)
                throw new PrisePluginException($@"Plugins must either provide a default parameterless constructor or implement a static factory method.
                    Example; 'public static {pluginType.Name} CreatePlugin(IServiceProvider serviceProvider)");

            var hostServices = this.sharedServicesProvider.ProvideHostServices();
            var sharedServices = this.sharedServicesProvider.ProvideSharedServices();
            var allServices = new ServiceCollection();

            foreach (var service in hostServices)
                allServices.Add(service);

            foreach (var service in sharedServices)
                allServices.Add(service);

            if (bootstrapper != null)
                sharedServices = bootstrapper.Bootstrap(allServices);

            allServices.AddScoped<IPluginServiceProvider>(sp => new DefaultPluginServiceProvider(
                sp,
                hostServices.Select(d => d.ServiceType),
                sharedServices.Select(d => d.ServiceType)
            ));

            var localProvider = allServices.BuildServiceProvider();

            if (pluginActivationContext.PluginFactoryMethodWithPluginServiceProvider != null)
            {
                var factoryMethodWithPluginServiceProvider = pluginActivationContext.PluginFactoryMethodWithPluginServiceProvider;

                return factoryMethodWithPluginServiceProvider.Invoke(null, new[] { localProvider.GetService<IPluginServiceProvider>() });
            }

            return factoryMethod.Invoke(null, new[] { localProvider });
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