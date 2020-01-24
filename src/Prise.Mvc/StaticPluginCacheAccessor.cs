using Prise.Infrastructure;

namespace Prise.Mvc
{
    public static class StaticPluginCacheAccessor<T>
    {
        public static IPluginCache<T> CurrentCache { get; internal set; }
    }
}
