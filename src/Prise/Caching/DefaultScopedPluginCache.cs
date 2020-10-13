using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace Prise.Caching
{
    public class DefaultScopedPluginCache : IPluginCache
    {
        protected ConcurrentBag<ICachedPluginAssembly> pluginCache;

        public DefaultScopedPluginCache()
        {
            this.pluginCache = new ConcurrentBag<ICachedPluginAssembly>();
        }

        public void Add(IAssemblyShim pluginAssembly, IEnumerable<Type> hostTypes = null)
        {
            this.pluginCache.Add(new CachedPluginAssembly
            {
                AssemblyShim = pluginAssembly,
                HostTypes = hostTypes
            });
        }

        public void Remove(string assemblyName)
        {
            this.pluginCache = new ConcurrentBag<ICachedPluginAssembly>(this.pluginCache.Where(a => a.AssemblyShim.Assembly.GetName().Name != assemblyName));
        }

        public ICachedPluginAssembly[] GetAll() => this.pluginCache.ToArray();
    }
}