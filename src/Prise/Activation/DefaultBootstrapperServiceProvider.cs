using Prise.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Prise.Activation
{
    public class DefaultBootstrapperServiceProvider : IBootstrapperServiceProvider
    {
        protected IServiceProvider localProvider;
        protected IEnumerable<Type> hostTypes;
        protected ConcurrentBag<object> instances;

        public DefaultBootstrapperServiceProvider(IServiceProvider localProvider, IEnumerable<Type> hostTypes)
        {
            this.localProvider = localProvider.ThrowIfNull(nameof(localProvider));
            this.hostTypes = hostTypes.ThrowIfNull(nameof(hostTypes));
            this.instances = new ConcurrentBag<object>();
        }

        public virtual object GetHostService(Type type)
        {
            var hostType = this.hostTypes.FirstOrDefault(t => t.Name == type.Name);
            if (hostType == null)
                throw new PluginActivationException($"An instance of type {type.Name} is required to activate this plugin, but it was not registered as a Host Type, please configure this type via the UseHostServices, ConfigureHostServices or ConfigureSharedServices builder method.");

            var instance = this.localProvider.GetService(hostType);
            if (instance == null)
                throw new PluginActivationException($"An instance of Host Service type {type.Name} could not be properly constructed (null).");

            this.instances.Add(instance);
            return instance;
        }

        protected volatile bool disposed;

        public virtual void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            foreach (object instance in instances)
            {
                (instance as IDisposable)?.Dispose();
            }

            instances?.Clear();

            instances = null;
            localProvider = null;
            hostTypes = null;
        }
    }
}