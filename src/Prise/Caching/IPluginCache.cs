using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace Prise.Caching
{
    public interface IPluginCache
    {
        void Add(IAssemblyShim pluginAssembly, IEnumerable<Type> hostTypes = null);
        void Remove(string assemblyName);
        ICachedPluginAssembly[] GetAll();
    }
}