using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Prise.Plugin;

namespace Prise.Activation
{
    public class DefaultRemotePluginActivator : IRemotePluginActivator
    {
        private Func<IServiceProvider, IEnumerable<Type>, IBootstrapperServiceProvider> bootstrapperServiceProviderFactory;
        private Func<IServiceProvider, IEnumerable<Type>, IEnumerable<Type>, IPluginServiceProvider> pluginServiceProviderFactory;
        private ConcurrentBag<object> instances;

        public DefaultRemotePluginActivator(Func<IServiceProvider, IEnumerable<Type>, IBootstrapperServiceProvider> bootstrapperServiceProviderFactory,
                                            Func<IServiceProvider, IEnumerable<Type>, IEnumerable<Type>, IPluginServiceProvider> pluginServiceProviderFactory)
        {
            this.bootstrapperServiceProviderFactory = bootstrapperServiceProviderFactory;
            this.pluginServiceProviderFactory = pluginServiceProviderFactory;
            this.instances = new ConcurrentBag<object>();
        }

        private object AddToDisposables(object obj)
        {
            this.instances.Add(obj);
            return obj;
        }

        public virtual object CreateRemoteBootstrapper(IPluginActivationContext pluginActivationContext, IServiceCollection hostServices = null)
        {
            var bootstrapperType = pluginActivationContext.PluginBootstrapperType;
            var assembly = pluginActivationContext.PluginAssembly.Assembly;
            var bootstrapperServices = pluginActivationContext.BootstrapperServices;

            var contructors = bootstrapperType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var firstCtor = contructors.First();

            if (!contructors.Any())
                throw new PluginActivationException($"No public constructors found for remote bootstrapper {bootstrapperType.Name}");
            if (firstCtor.GetParameters().Any())
                throw new PluginActivationException($"Bootstrapper {bootstrapperType.Name} must contain a public parameterless constructor");
            var bootstrapperInstance = assembly.CreateInstance(bootstrapperType.FullName);
            var serviceProvider = AddToDisposables(GetServiceProviderForBootstrapper(new ServiceCollection(), hostServices ?? new ServiceCollection())) as IServiceProvider;
            var bootstrapperServiceProvider = AddToDisposables(serviceProvider.GetService<IBootstrapperServiceProvider>()) as IBootstrapperServiceProvider;

            bootstrapperInstance = InjectBootstrapperFieldsWithServices(bootstrapperInstance, bootstrapperServiceProvider, bootstrapperServices);

            return AddToDisposables(bootstrapperInstance);
        }

        public virtual object CreateRemoteInstance(IPluginActivationContext pluginActivationContext, IPluginBootstrapper bootstrapper = null, IServiceCollection hostServices = null)
        {
            var pluginType = pluginActivationContext.PluginType;
            var pluginAssembly = pluginActivationContext.PluginAssembly;
            var factoryMethod = pluginActivationContext.PluginFactoryMethod;

            var contructors = pluginType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            if (contructors.Count() > 1)
                throw new PluginActivationException($"Multiple public constructors found for remote plugin {pluginType.Name}");

            var serviceProvider = AddToDisposables(GetServiceProviderForPlugin(new ServiceCollection(), bootstrapper, hostServices ?? new ServiceCollection())) as IServiceProvider;

            if (factoryMethod != null)
                return AddToDisposables(factoryMethod.Invoke(null, new[] { serviceProvider }));

            var firstCtor = contructors.FirstOrDefault();
            if (firstCtor != null && !firstCtor.GetParameters().Any()) // Empty default CTOR
            {
                var pluginServiceProvider = AddToDisposables(serviceProvider.GetService<IPluginServiceProvider>()) as IPluginServiceProvider;
                var remoteInstance = pluginAssembly.Assembly.CreateInstance(pluginType.FullName);
                remoteInstance = InjectPluginFieldsWithServices(remoteInstance, pluginServiceProvider, pluginActivationContext.PluginServices);

                ActivateIfNecessary(remoteInstance, pluginActivationContext);

                return AddToDisposables(remoteInstance);
            }

            throw new PluginActivationException($"Plugin of type {pluginType.Name} could not be activated.");
        }

        protected virtual void ActivateIfNecessary(object remoteInstance, IPluginActivationContext pluginActivationContext)
        {
            var pluginType = pluginActivationContext.PluginType;
            if (pluginActivationContext.PluginActivatedMethod == null)
                return;

            var remoteActivationMethod = remoteInstance.GetType().GetRuntimeMethods().FirstOrDefault(m => m.Name == pluginActivationContext.PluginActivatedMethod.Name);
            if (remoteActivationMethod == null)
                throw new PluginActivationException($"Remote activation method {pluginActivationContext.PluginActivatedMethod.Name} not found for plugin {pluginType.Name} on remote object {remoteInstance.GetType().Name}");

            remoteActivationMethod.Invoke(remoteInstance, null);
        }

        protected virtual object InjectBootstrapperFieldsWithServices(object remoteInstance,
                                                                      IBootstrapperServiceProvider bootstrapperServiceProvider,
                                                                      IEnumerable<BootstrapperService> bootstrapperServices)
        {
            foreach (var bootstrapperService in bootstrapperServices)
            {
                var fieldName = bootstrapperService.FieldName;
                var serviceInstance = bootstrapperServiceProvider.GetHostService(bootstrapperService.ServiceType);

                if (bootstrapperService.BridgeType == null)
                    throw new PluginActivationException($"Field {fieldName} requires a BridgeType.");

                var bridgeConstructor = GetBridgeConstructor(bootstrapperService.BridgeType);
                if (bridgeConstructor == null)
                    throw new PluginActivationException($"PluginBridge {bootstrapperService.BridgeType.Name} must have a single public constructor with one parameter of type object.");

                var bridgeInstance = AddToDisposables(bridgeConstructor.Invoke(new[] { serviceInstance }));
                if (!TrySetField(remoteInstance, fieldName, bridgeInstance))
                    throw new PluginActivationException($"Field {bootstrapperService.FieldName} on Bootstrapper could not be set.");
            }

            return remoteInstance;
        }

        protected virtual object InjectPluginFieldsWithServices(object remoteInstance,
                                                                IPluginServiceProvider pluginServiceProvider,
                                                                IEnumerable<PluginService> pluginServices)
        {
            foreach (var pluginService in pluginServices)
            {
                var fieldName = pluginService.FieldName;
                object serviceInstance = null;
                switch (pluginService.ProvidedBy)
                {
                    case ProvidedBy.Host:
                        serviceInstance = pluginServiceProvider.GetHostService(pluginService.ServiceType);
                        break;
                    case ProvidedBy.Plugin:
                        serviceInstance = pluginServiceProvider.GetPluginService(pluginService.ServiceType);
                        break;
                }

                if (TrySetField(remoteInstance, fieldName, serviceInstance))
                    continue; // Field was set successfully, continueing

                if (pluginService.BridgeType == null)
                    throw new PluginActivationException($"Field {pluginService.FieldName} could not be set, please consider using a PluginBridge.");

                var bridgeConstructor = GetBridgeConstructor(pluginService.BridgeType);
                if (bridgeConstructor == null)
                    throw new PluginActivationException($"PluginBridge {pluginService.BridgeType.Name} must have a single public constructor with one parameter of type object.");

                var bridgeInstance = AddToDisposables(bridgeConstructor.Invoke(new[] { serviceInstance }));
                if (!TrySetField(remoteInstance, fieldName, bridgeInstance))
                    throw new PluginActivationException($"Field {pluginService.FieldName} on Plugin could not be set.");
            }

            return remoteInstance;
        }

        protected virtual IServiceProvider GetServiceProviderForBootstrapper(IServiceCollection services, IServiceCollection hostServices)
        {
            foreach (var service in hostServices)
                services.Add(service);

            services.AddScoped<IBootstrapperServiceProvider>(sp => this.bootstrapperServiceProviderFactory(
                sp,
                hostServices.Select(d => d.ServiceType)
            ));

            return services.BuildServiceProvider();
        }

        protected virtual IServiceProvider GetServiceProviderForPlugin(IServiceCollection services, IPluginBootstrapper bootstrapper, IServiceCollection hostServices)
        {
            // Add all the host services to the main collection
            foreach (var service in hostServices)
                services.Add(service);

            IServiceCollection pluginServices = new ServiceCollection();
            // If a bootstrapper was provided, add the services for the plugin to a new collection
            if (bootstrapper != null)
                pluginServices = bootstrapper.Bootstrap(pluginServices);

            // Add all the plugin services to the main collection
            foreach (var service in pluginServices)
                services.Add(service);

            services.AddScoped<IPluginServiceProvider>(sp => this.pluginServiceProviderFactory(
                sp,
                hostServices.Select(d => d.ServiceType),
                pluginServices.Select(d => d.ServiceType)
            ));

            return services.BuildServiceProvider();
        }

        protected virtual ConstructorInfo GetBridgeConstructor(Type bridgeType)
        {
            return bridgeType
                       .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                       .FirstOrDefault(c => c.GetParameters().Count() == 1 && c.GetParameters().First().ParameterType == typeof(object));
        }

        protected virtual bool TrySetField(object remoteInstance, string fieldName, object fieldInstance)
        {
            try
            {
                remoteInstance
                    .GetType()
                    .GetTypeInfo()
                        .DeclaredFields
                            .First(f => f.Name == fieldName)
                            .SetValue(remoteInstance, fieldInstance);
                return true;
            }
            catch (ArgumentException) { }
            return false;
        }


        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                foreach (var disposable in this.instances)
                {
                    if (disposable as IDisposable != null)
                        (disposable as IDisposable)?.Dispose();
                }
                instances.Clear();

                this.instances = null;
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