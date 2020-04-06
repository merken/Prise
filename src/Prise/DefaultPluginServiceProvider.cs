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
            var instance = this.localProvider.GetService(type);

            return instance;
        }

        public object GetHostService(Type type)
        {
            var hostType = this.hostTypes.FirstOrDefault(t => t.Name == type.Name);
            var sharedType = this.sharedTypes.FirstOrDefault(t => t.Name == type.Name);
            //if (hostType == null)
            //    throw new PrisePluginException($"An instance of type {type.Name} is required to activate this plugin, but it was not registered as a Shared Type, please configure this type in the ConfigureSharedServices builder.");

            var instance = this.localProvider.GetService(hostType ?? sharedType);

            return instance;
        }
    }
}
