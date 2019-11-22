using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prise.Plugin;
using System.Net.Http;

namespace HttpPlugin
{
    [PluginBootstrapper(PluginType = typeof(MyProductsRepository))]
    [PluginBootstrapper(PluginType = typeof(SWAPIRepository))]
    public class HttpPluginBootstrapper : IPluginBootstrapper
    {
        public IServiceCollection Bootstrap(IServiceCollection services)
        {
            var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            var httpConfig = new HttpConfig();
            config.Bind("HttpPlugin", httpConfig);

            services.AddSingleton<HttpConfig>(httpConfig);

            services.AddScoped<HttpClient>((serviceProvider) =>
            {
                var client = new HttpClient();
                return client;
            });

            return services;
        }
    }
}
