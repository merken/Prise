using System;
using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prise.Plugin;

namespace TableStoragePlugin
{
    [PluginBootstrapper(PluginType = typeof(TableStorageProductsRepository))]
    public class TableStorageBootstrapper : IPluginBootstrapper
    {
        public IServiceCollection Bootstrap(IServiceCollection services)
        {
            var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            var tableStorageConfig = new TableStorageConfig();
            config.Bind("TableStoragePlugin", tableStorageConfig);
            
            services.AddScoped<TableStorageConfig>((serviceProvider) =>
            {
                return tableStorageConfig;
            });

            return services;
        }
    }
}