using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Prise.Activation
{
    public class DefaultPluginServiceProvider : IPluginServiceProvider
    {
        private IServiceProvider localProvider;
        private IEnumerable<Type> hostTypes;
        private IEnumerable<Type> sharedTypes;
        private ConcurrentBag<object> instances;

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

        protected bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                foreach (var instance in this.instances)
                    (instance as IDisposable)?.Dispose();

                this.instances.Clear();

                this.instances = null;
                this.localProvider = null;
                this.hostTypes = null;
                this.sharedTypes = null;
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