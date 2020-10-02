using Prise.Caching;

namespace Prise.Mvc
{
    public interface IPluginCacheAccessorBootstrapper
    {
        void SetCurrentCache(IPluginCache cache);
    }
}
