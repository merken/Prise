using System;
using System.Collections.Generic;
using System.Linq;
using Prise.Infrastructure;

namespace Prise
{
    public class DefaultPluginServiceProvider : IPluginServiceProvider
    {
        private readonly IServiceProvider localProvider;
        private readonly IEnumerable<Type> hostTypes;
        private readonly IEnumerable<Type> sharedTypes;

        public DefaultPluginServiceProvider(IServiceProvider localProvider, IEnumerable<Type> hostTypes, IEnumerable<Type> sharedTypes)
        {
            this.localProvider = localProvider;
            this.hostTypes = hostTypes;
            this.sharedTypes = sharedTypes;
        }

        public object GetPluginService(Type type)
        {
            // Plugin services are registered via a PluginBootstrapper, eventually they'll land inside the localProvider.
            var sharedType = this.sharedTypes.FirstOrDefault(t => t.Name == type.Name);
            if (sharedType == null)
                throw new PrisePluginException($"An instance of type {type.Name} is required to activate this plugin, but it was not registered as a Plugin Type, please provide this service via a PluginBootstrapper builder.");

            var instance = this.localProvider.GetService(type);
            if (instance == null)
                throw new PrisePluginException($"An instance of Plugin Service type {type.Name} could not be properly constructed (null).");

            return instance;
        }

        public object GetHostService(Type type)
        {
            // Host Services can either be provided as Shared Service or Host Service
            var hostType = this.hostTypes.FirstOrDefault(t => t.Name == type.Name);
            var sharedType = this.sharedTypes.FirstOrDefault(t => t.Name == type.Name);
            if ((hostType ?? sharedType) == null)
                throw new PrisePluginException($"An instance of type {type.Name} is required to activate this plugin, but it was not registered as a Shared Type or a Host Type, please configure this type via the UseHostServices, ConfigureHostServices or ConfigureSharedServices builder method.");

            var instance = this.localProvider.GetService(hostType ?? sharedType);
            if (instance == null)
                throw new PrisePluginException($"An instance of Host Service type {type.Name} could not be properly constructed (null).");

            return instance;
        }
    }
}
