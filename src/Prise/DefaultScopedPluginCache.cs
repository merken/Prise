using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Prise.Infrastructure;

namespace Prise
{
    public class CacheOptions<IPluginCache>
    {
        private readonly ServiceLifetime serviceLifetime;
        public CacheOptions(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            this.serviceLifetime = serviceLifetime;
        }

        public ServiceLifetime Lifetime => this.serviceLifetime;

        public static CacheOptions<IPluginCache> ScopedPluginCache() => new CacheOptions<IPluginCache>(ServiceLifetime.Scoped);
        public static CacheOptions<IPluginCache> SingletonPluginCache() => new CacheOptions<IPluginCache>(ServiceLifetime.Singleton);
    }

    public class DefaultScopedPluginCache<T> : IPluginCache<T>
    {
        protected ConcurrentBag<Assembly> pluginCache;

        public DefaultScopedPluginCache()
        {
            this.pluginCache = new ConcurrentBag<Assembly>();
        }

        public void Add(Assembly pluginAssembly)
        {
            this.pluginCache.Add(pluginAssembly);
        }

        public void Remove(string assemblyName)
        {
            this.pluginCache = new ConcurrentBag<Assembly>(this.pluginCache.Where(a => a.GetName().Name != assemblyName));
        }

        public Assembly[] GetAll() => this.pluginCache.ToArray();
    }
}
