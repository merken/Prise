using System;
using System.Collections.Generic;
using Prise.Infrastructure;

namespace Prise
{
    public class DefaultPluginSelector<T> : IPluginSelector<T>
    {
        private readonly Func<IEnumerable<Type>, IEnumerable<Type>> pluginSelector;

        public DefaultPluginSelector(Func<IEnumerable<Type>, IEnumerable<Type>> pluginSelector = null)
        {
            this.pluginSelector = pluginSelector;
        }

        public IEnumerable<Type> SelectPlugins(IEnumerable<Type> pluginTypes)
        {
            if (pluginSelector == null)
                return pluginTypes;
            return pluginSelector.Invoke(pluginTypes);
        }
    }
}
