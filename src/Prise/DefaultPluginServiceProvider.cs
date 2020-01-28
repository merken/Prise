using System;
using System.Collections.Generic;
using System.Linq;
using Prise.Infrastructure;

namespace Prise
{
    public class DefaultPluginServiceProvider : IPluginServiceProvider
    {
        private readonly IServiceProvider localProvider;
        private readonly IEnumerable<Type> sharedTypes;

        public DefaultPluginServiceProvider(IServiceProvider localProvider, IEnumerable<Type> hostTypes, IEnumerable<Type> sharedTypes)
        {
            this.localProvider = localProvider;
            this.sharedTypes = sharedTypes;
        }

        public object GetPluginService(Type type)
        {
            var instance = this.localProvider.GetService(type);

            return instance;
        }

        public object GetHostService(Type type)
        {
            var instanceType = this.sharedTypes.FirstOrDefault(t => t.Name == type.Name);
            var instance = this.localProvider.GetService(instanceType);

            return instance;
        }
    }
}
