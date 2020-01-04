using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prise.Plugin;
using TableStorageConnector;

namespace ProductsReaderPlugin
{
    [PluginBootstrapper(PluginType = typeof(TableStorageProductsReader))]
    public class TableStorageProductsReaderBootstrapper : IPluginBootstrapper
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
