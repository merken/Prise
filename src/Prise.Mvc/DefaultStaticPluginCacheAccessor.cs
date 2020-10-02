using Prise.Caching;

namespace Prise.Mvc
{
    public static class DefaultStaticPluginCacheAccessor
    {
        public static IPluginCache CurrentCache { get; internal set; }
    }
}
