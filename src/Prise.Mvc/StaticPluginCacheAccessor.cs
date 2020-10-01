using Prise.Caching;

namespace Prise.Mvc
{
    public static class StaticPluginCacheAccessor
    {
        public static IPluginCache CurrentCache { get; internal set; }
    }
}
