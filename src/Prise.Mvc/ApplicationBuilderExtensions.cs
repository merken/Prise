using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prise.Mvc.Infrastructure;

namespace Prise.Mvc
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder EnsureStaticPluginCache<T>(this IApplicationBuilder app)
        {
            app.ApplicationServices.GetRequiredService<IPluginCacheAccessorBootstrapper<T>>();
            return app;
        }
    }
}
