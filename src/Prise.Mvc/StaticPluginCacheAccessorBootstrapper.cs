using System;
using Prise.Caching;

namespace Prise.Mvc
{
    public interface IPluginCacheAccessorBootstrapper
    {
        void SetCurrentCache(IPluginCache cache);
    }

    public class StaticPluginCacheAccessorBootstrapper : IPluginCacheAccessorBootstrapper
    {
        protected bool isBootstrapped;
        public StaticPluginCacheAccessorBootstrapper(IPluginCache cache)
        {
            if (this.isBootstrapped)
                throw new NotSupportedException($"IPluginCache was already bootstrapped");

            this.SetCurrentCache(cache);
            this.isBootstrapped = true;
        }

        public void SetCurrentCache(IPluginCache cache)
        {
            StaticPluginCacheAccessor.CurrentCache = cache;
        }
    }
}
