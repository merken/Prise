using System;
using System.Collections.Generic;

namespace Prise.Infrastructure
{
    public interface IPluginSelector<T>
    {
        IEnumerable<Type> SelectPlugins(IEnumerable<Type> pluginTypes);
    }
}
