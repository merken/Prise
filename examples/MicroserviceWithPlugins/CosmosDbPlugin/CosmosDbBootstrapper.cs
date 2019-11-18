using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prise.Plugin;
using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosDbPlugin
{
    [PluginBootstrapper(PluginType = typeof(CosmosDbProductsRepository))]
    public class CosmosDbBootstrapper : IPluginBootstrapper
    {
        public IServiceCollection Bootstrap(IServiceCollection services)
        {
            var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            var cosmosDbConfig = new CosmosDbConfig();
            config.Bind("CosmosDbPlugin", cosmosDbConfig);

            services.AddScoped<CosmosDbConfig>((serviceProvider) =>
            {
                return cosmosDbConfig;
            });

            return services;
        }
    }
}
