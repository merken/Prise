using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prise.Plugin;
using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosDbPlugin
{
    [PluginBootstrapper(PluginType = typeof(MongoDbProductsRepository))]
    public class MongoDbBootstrapper : IPluginBootstrapper
    {
        public IServiceCollection Bootstrap(IServiceCollection services)
        {
            var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            var mongoDbConfig = new MongoDbConfig();
            config.Bind("MongoDbPlugin", mongoDbConfig);

            services.AddScoped<MongoDbConfig>((serviceProvider) =>
            {
                return mongoDbConfig;
            });

            return services;
        }
    }
}
