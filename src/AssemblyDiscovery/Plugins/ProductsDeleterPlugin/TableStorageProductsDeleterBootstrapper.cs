using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prise.Plugin;
using TableStorageConnector;

namespace ProductsDeleterPlugin
{
    [PluginBootstrapper(PluginType = typeof(TableStorageProductsDeleter))]
    public class TableStorageProductsDeleterBootstrapper : IPluginBootstrapper
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
