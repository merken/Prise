using System;
using System.Collections.Generic;
using System.Text;

namespace Prise.Infrastructure
{
    public interface IPluginSelector
    {
        IEnumerable<Type> SelectPlugins(IEnumerable<Type> pluginTypes);
    }

    public class DefaultPluginSelector : IPluginSelector
    {
        public IEnumerable<Type> SelectPlugins(IEnumerable<Type> pluginTypes)
        {
            return pluginTypes;
        }
    }

    public class PluginSelector : IPluginSelector
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
