using System;
using System.Collections.Generic;

namespace Prise.Infrastructure
{
    public interface IPluginSelector<T>
    {
        IEnumerable<Type> SelectPlugins(IEnumerable<Type> pluginTypes);
    }

    public class DefaultPluginSelector<T> : IPluginSelector<T>
    {
        public IEnumerable<Type> SelectPlugins(IEnumerable<Type> pluginTypes)
        {
            return pluginTypes;
        }
    }

    public class PluginSelector<T> : IPluginSelector<T>
    {
        private readonly Func<IEnumerable<Type>, IEnumerable<Type>> pluginSelector;

        public PluginSelector(Func<IEnumerable<Type>, IEnumerable<Type>> pluginSelector)
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
