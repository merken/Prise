using Prise.Infrastructure;

namespace Prise.Mvc.Infrastructure
{
    public interface IPluginCacheAccessorBootstrapper<T>
    {
        void SetCurrentCache(IPluginCache<T> cache);
    }
}
