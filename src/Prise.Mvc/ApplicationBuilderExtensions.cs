using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Prise.Mvc
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder EnsureStaticPluginCache(this IApplicationBuilder app)
        {
            app.ApplicationServices.GetRequiredService<IPluginCacheAccessorBootstrapper>();
            return app;
        }
    }
}
