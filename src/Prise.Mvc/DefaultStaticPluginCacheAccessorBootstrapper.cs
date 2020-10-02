using System;
using Prise.Caching;

namespace Prise.Mvc
{
    public class DefaultStaticPluginCacheAccessorBootstrapper : IPluginCacheAccessorBootstrapper
    {
        protected bool isBootstrapped;
        public DefaultStaticPluginCacheAccessorBootstrapper(IPluginCache cache)
        {
            if (this.isBootstrapped)
                throw new NotSupportedException($"IPluginCache was already bootstrapped");

            this.SetCurrentCache(cache);
            this.isBootstrapped = true;
        }

        public void SetCurrentCache(IPluginCache cache)
        {
            DefaultStaticPluginCacheAccessor.CurrentCache = cache;
        }
    }
}
