using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Prise.Plugin;
using Prise.Proxy;

namespace Prise.V2
{
    public class JsonSerializerResultConverter : ResultConverter
    {
        public override object Deserialize(Type localType, Type remoteType, object value)
        {
            // Get the local type
            var resultType = localType;
            // Check if the type is a Task<T>
            if (localType.BaseType == typeof(System.Threading.Tasks.Task))
            {
                // Get the <T>
                resultType = localType.GenericTypeArguments[0];
            }

            return JsonSerializer.Deserialize(
                    JsonSerializer.Serialize(value), // First, serialize the object into a string
                    resultType); // Second, deserialize it using the correct type
        }
    }

    public class JsonSerializerParameterConverter : IParameterConverter
    {
        protected bool disposed = false;

        public object ConvertToRemoteType(Type localType, object value)
        {
            var json = JsonSerializer.Serialize(value);
            return JsonSerializer.Deserialize(json, localType);
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

    public interface IPluginProxyCreator : IDisposable
    {
        IPluginBootstrapper CreateBootstrapperProxy(object remoteBootstrapper);
        T CreatePluginProxy<T>(object remoteObject, IParameterConverter parameterConverter, IResultConverter resultConverter);
    }

    public class DefaultPluginProxyCreator : IPluginProxyCreator
    {
        protected bool disposed = false;

        public IPluginBootstrapper CreateBootstrapperProxy(object remoteBootstrapper) =>
            ProxyCreator.CreateProxy<IPluginBootstrapper>(remoteBootstrapper);

        public T CreatePluginProxy<T>(object remoteObject, IParameterConverter parameterConverter, IResultConverter resultConverter) =>
            ProxyCreator.CreateProxy<T>(remoteObject, parameterConverter, resultConverter);

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
    public interface IPluginServiceProvider : IDisposable
    {
        object GetPluginService(Type type);
        object GetHostService(Type type);
    }

    public class DefaultPluginServiceProvider : IPluginServiceProvider
    {
        protected bool disposed = false;
        private readonly IServiceProvider localProvider;
        private readonly IEnumerable<Type> hostTypes;
        private readonly IEnumerable<Type> sharedTypes;
        private readonly ConcurrentBag<object> instances;

        public DefaultPluginServiceProvider(IServiceProvider localProvider, IEnumerable<Type> hostTypes, IEnumerable<Type> sharedTypes)
        {
            this.localProvider = localProvider;
            this.hostTypes = hostTypes;
            this.sharedTypes = sharedTypes;
            this.instances = new ConcurrentBag<object>();
        }

        public object GetPluginService(Type type)
        {
            // Plugin services are registered via a PluginBootstrapper, eventually they'll land inside the localProvider.
            var sharedType = this.sharedTypes.FirstOrDefault(t => t.Name == type.Name);
            if (sharedType == null)
                throw new PluginActivationException($"An instance of type {type.Name} is required to activate this plugin, but it was not registered as a Plugin Type, please provide this service via a PluginBootstrapper builder.");

            var instance = this.localProvider.GetService(type);
            if (instance == null)
                throw new PluginActivationException($"An instance of Plugin Service type {type.Name} could not be properly constructed (null).");

            this.instances.Add(instance);
            return instance;
        }

        public object GetHostService(Type type)
        {
            // Host Services can either be provided as Shared Service or Host Service
            var hostType = this.hostTypes.FirstOrDefault(t => t.Name == type.Name);
            var sharedType = this.sharedTypes.FirstOrDefault(t => t.Name == type.Name);
            if ((hostType ?? sharedType) == null)
                throw new PluginActivationException($"An instance of type {type.Name} is required to activate this plugin, but it was not registered as a Shared Type or a Host Type, please configure this type via the UseHostServices, ConfigureHostServices or ConfigureSharedServices builder method.");

            var instance = this.localProvider.GetService(hostType ?? sharedType);
            if (instance == null)
                throw new PluginActivationException($"An instance of Host Service type {type.Name} could not be properly constructed (null).");

            this.instances.Add(instance);
            return instance;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                foreach (var instance in this.instances)
                    (instance as IDisposable)?.Dispose();

                this.instances.Clear();
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public interface IRemotePluginActivator : IDisposable
    {
        object CreateRemoteBootstrapper(Type bootstrapperType, Assembly assembly);
        object CreateRemoteInstance(PluginActivationContext pluginActivationContext, IPluginBootstrapper bootstrapper = null);
    }

    public class DefaultRemotePluginActivator : IRemotePluginActivator
    {
        private ConcurrentBag<object> instances;
        private IServiceCollection sharedServices;
        private IServiceCollection hostServices;
        private IServiceCollection services;
        private bool disposed = false;

        public DefaultRemotePluginActivator(IServiceCollection sharedServices, IServiceCollection hostServices)
        {
            this.sharedServices = sharedServices ?? new ServiceCollection();
            this.hostServices = hostServices ?? new ServiceCollection();
            this.instances = new ConcurrentBag<object>();
            this.services = new ServiceCollection();
        }

        private object AddToDisposables(object obj)
        {
            this.instances.Add(obj);
            return obj;
        }

        public virtual object CreateRemoteBootstrapper(Type bootstrapperType, Assembly assembly)
        {
            var contructors = bootstrapperType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var firstCtor = contructors.First();

            if (!contructors.Any())
                throw new PluginActivationException($"No public constructors found for remote bootstrapper {bootstrapperType.Name}");
            if (firstCtor.GetParameters().Any())
                throw new PluginActivationException($"Bootstrapper {bootstrapperType.Name} must contain a public parameterless constructor");

            return AddToDisposables(assembly.CreateInstance(bootstrapperType.FullName));
        }

        public virtual object CreateRemoteInstance(PluginActivationContext pluginActivationContext, IPluginBootstrapper bootstrapper = null)
        {
            var pluginType = pluginActivationContext.PluginType;
            var pluginAssembly = pluginActivationContext.PluginAssembly;
            var factoryMethod = pluginActivationContext.PluginFactoryMethod;

            var contructors = pluginType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            if (contructors.Count() > 1)
                throw new PluginActivationException($"Multiple public constructors found for remote plugin {pluginType.Name}");

            var serviceProvider = AddToDisposables(GetServiceProvider(bootstrapper)) as IServiceProvider;

            if (factoryMethod != null)
                return AddToDisposables(factoryMethod.Invoke(null, new[] { serviceProvider }));

            var firstCtor = contructors.FirstOrDefault();
            if (firstCtor != null && !firstCtor.GetParameters().Any()) // Empty default CTOR
            {
                var pluginServiceProvider = AddToDisposables(serviceProvider.GetService<IPluginServiceProvider>()) as IPluginServiceProvider;
                var remoteInstance = pluginAssembly.CreateInstance(pluginType.FullName);
                remoteInstance = InjectFieldsWithServices(remoteInstance, pluginServiceProvider, pluginActivationContext.PluginServices);

                ActivateIfNecessary(remoteInstance, pluginActivationContext);

                return AddToDisposables(remoteInstance);
            }

            throw new PluginActivationException($"Plugin of type {pluginType.Name} could not be activated.");
        }

        protected virtual void ActivateIfNecessary(object remoteInstance, PluginActivationContext pluginActivationContext)
        {
            var pluginType = pluginActivationContext.PluginType;
            if (pluginActivationContext.PluginActivatedMethod == null)
                return;

            var remoteActivationMethod = remoteInstance.GetType().GetRuntimeMethods().FirstOrDefault(m => m.Name == pluginActivationContext.PluginActivatedMethod.Name);
            if (remoteActivationMethod == null)
                throw new PluginActivationException($"Remote activation method {pluginActivationContext.PluginActivatedMethod.Name} not found for plugin {pluginType.Name} on remote object {remoteInstance.GetType().Name}");

            remoteActivationMethod.Invoke(remoteInstance, null);
        }

        protected virtual object InjectFieldsWithServices(object remoteInstance, IPluginServiceProvider pluginServiceProvider, IEnumerable<PluginService> pluginServices)
        {
            foreach (var pluginService in pluginServices)
            {
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

                try
                {
                    remoteInstance
                        .GetType()
                        .GetTypeInfo()
                            .DeclaredFields
                                .First(f => f.Name == pluginService.FieldName)
                                .SetValue(remoteInstance, serviceInstance);
                    continue;
                }
                catch (ArgumentException) { }

                if (pluginService.BridgeType == null)
                    throw new PluginActivationException($"Field {pluginService.FieldName} could not be set, please consider using a PluginBridge.");

                var bridgeConstructor = pluginService.BridgeType
                        .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                        .FirstOrDefault(c => c.GetParameters().Count() == 1 && c.GetParameters().First().ParameterType == typeof(object));

                if (bridgeConstructor == null)
                    throw new PluginActivationException($"PluginBridge {pluginService.BridgeType.Name} must have a single public constructor with one parameter of type object.");

                var bridgeInstance = AddToDisposables(bridgeConstructor.Invoke(new[] { serviceInstance }));
                remoteInstance.GetType().GetTypeInfo().DeclaredFields.First(f => f.Name == pluginService.FieldName).SetValue(remoteInstance, bridgeInstance);
            }

            return remoteInstance;
        }

        protected virtual IServiceProvider GetServiceProvider(IPluginBootstrapper bootstrapper)
        {
            var hostServices = this.hostServices;
            var sharedServices = this.sharedServices;

            foreach (var service in hostServices)
                this.services.Add(service);

            foreach (var service in sharedServices)
                this.services.Add(service);

            if (bootstrapper != null)
                sharedServices = bootstrapper.Bootstrap(this.services);

            this.services.AddScoped<IPluginServiceProvider>(sp => new DefaultPluginServiceProvider(
                sp,
                hostServices.Select(d => d.ServiceType),
                sharedServices.Select(d => d.ServiceType)
            ));

            return this.services.BuildServiceProvider();
        }

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
                this.services = null;
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
    public enum ProvidedBy
    {
        Plugin = 0,
        Host
    }


    [Serializable]
    public class PluginActivationException : Exception
    {
        public PluginActivationException(string message) : base(message)
        {
        }

        public PluginActivationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public PluginActivationException()
        {
        }

        protected PluginActivationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class PluginService
    {
        public string FieldName { get; set; }
        public Type ServiceType { get; set; }
        public ProvidedBy ProvidedBy { get; set; }
        public Type BridgeType { get; set; }
    }

    public class PluginActivationContext
    {
        public Assembly PluginAssembly { get; set; }
        public Type PluginType { get; set; }
        public Type PluginBootstrapperType { get; set; }
        public MethodInfo PluginFactoryMethod { get; set; }
        public MethodInfo PluginActivatedMethod { get; set; }
        public IEnumerable<PluginService> PluginServices { get; set; }
    }

    public interface IPluginActivationContextProvider
    {
        PluginActivationContext ProvideActivationContext(Type remoteType, Assembly pluginAssembly);
    }

    public class DefaultPluginActivationContextProvider : IPluginActivationContextProvider
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
                pluginServices = pluginServiceFields.Select(f => ParsePluginService(remoteType, f)).ToList();
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
                throw new PluginActivationException($"The ServiceType {remoteType.Name} argument is required for the PluginServiceAttribute.");

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

    public interface IPluginActivator
    {
        Task<T> ActivatePlugin<T>(ref Assembly pluginAssembly,
                                  Type pluginType = null,
                                  IServiceCollection sharedServices = null,
                                  IServiceCollection hostServices = null,
                                  IParameterConverter parameterConverter = null,
                                  IResultConverter resultConverter = null);
    }

    public class DefaultPluginActivator : IPluginActivator
    {
        protected ConcurrentBag<IDisposable> disposables;

        public DefaultPluginActivator()
        {
            this.disposables = new ConcurrentBag<IDisposable>();
        }

        public Task<T> ActivatePlugin<T>(ref Assembly pluginAssembly,
                                         Type pluginType = null,
                                         IServiceCollection sharedServices = null,
                                         IServiceCollection hostServices = null,
                                         IParameterConverter parameterConverter = null,
                                         IResultConverter resultConverter = null)
        {
            parameterConverter = parameterConverter ?? new JsonSerializerParameterConverter();
            resultConverter = resultConverter ?? new JsonSerializerResultConverter();

            T pluginProxy = default(T);
            IPluginBootstrapper bootstrapperProxy = null;

            var pluginActivationContextProvider = new DefaultPluginActivationContextProvider();
            var pluginActivationContext = pluginActivationContextProvider.ProvideActivationContext(pluginType, pluginAssembly);

            var remoteActivator = new DefaultRemotePluginActivator(sharedServices, hostServices); // todo shared services
            var proxyCreator = new DefaultPluginProxyCreator();

            if (pluginActivationContext.PluginBootstrapperType != null)
            {
                var remoteBootstrapperInstance = remoteActivator.CreateRemoteBootstrapper(pluginActivationContext.PluginBootstrapperType, pluginAssembly);

                var remoteBootstrapperProxy = proxyCreator.CreateBootstrapperProxy(remoteBootstrapperInstance);

                this.disposables.Add(remoteBootstrapperProxy as IDisposable);
                bootstrapperProxy = remoteBootstrapperProxy;
            }

            var remoteObject = remoteActivator.CreateRemoteInstance(
                pluginActivationContext,
                bootstrapperProxy
            );

            pluginProxy = proxyCreator.CreatePluginProxy<T>(remoteObject, parameterConverter, resultConverter);

            this.disposables.Add(pluginProxy as IDisposable);

            return Task.FromResult(pluginProxy);
        }
    }
}