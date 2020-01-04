using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prise.Plugin;
using TableStorageConnector;

namespace ProductsWriterPlugin
{
    [PluginBootstrapper(PluginType = typeof(TableStorageProductsWriter))]
    public class TableStorageProductsWriterBootstrapper : IPluginBootstrapper
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
