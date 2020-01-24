using System;
using Prise.Infrastructure;
using Prise.Mvc.Infrastructure;

namespace Prise.Mvc
{
    public class StaticPluginCacheAccessorBootstrapper<T> : IPluginCacheAccessorBootstrapper<T>
    {
        protected bool isBootstrapped;
        public StaticPluginCacheAccessorBootstrapper(IPluginCache<T> cache)
        {
            if (this.isBootstrapped)
                throw new NotSupportedException($"IPluginCache<{typeof(T).Name}> was already bootstrapped");

            this.SetCurrentCache(cache);
            this.isBootstrapped = true;
        }

        public void SetCurrentCache(IPluginCache<T> cache)
        {
            StaticPluginCacheAccessor<T>.CurrentCache = cache;
        }
    }
}
