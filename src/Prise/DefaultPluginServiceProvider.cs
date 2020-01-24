using System;
using System.Collections.Generic;
using System.Linq;
using Prise.Plugin;

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

        public T GetPluginService<T>()
        {
            var instance = (T)this.localProvider.GetService(typeof(T));
            if (instance == null)
                throw new PrisePluginException($"Could not resolve Plugin Service <{typeof(T).Name}> Please check if this type is registered for plugin.");

            return instance;
        }

        public T GetHostService<T>()
        {
            var instance = (T)this.localProvider.GetService(typeof(T));
            if (instance == null)
                throw new PrisePluginException($"Could not resolve Host Service <{typeof(T).Name}> Could there be a version mismatch?");
            return instance;
        }

        public object GetSharedHostService(Type type)
        {
            var instanceType = this.sharedTypes.FirstOrDefault(t => t.Name == type.Name);
            var instance = this.localProvider.GetService(instanceType);
            if (instance == null)
                throw new PrisePluginException($"Could not resolve Shared Host Service <{type.Name}> Please check the registration.");

            return instance;
        }
    }
}
