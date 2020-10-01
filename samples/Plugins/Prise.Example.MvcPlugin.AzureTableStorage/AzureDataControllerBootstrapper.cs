
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Prise.Example.Contract;
using Microsoft.Extensions.DependencyInjection;
using Prise.PluginBridge;
using Prise.Plugin;

namespace Prise.Example.MvcPlugin.AzureTableStorage
{
    /// <summary>
    /// This bootstrapper will create the AzureDataController API Controller using DI
    /// </summary>
    [PluginBootstrapper(PluginType = typeof(AzureDataController))]
    public class AzureDataControllerBootstrapper : IPluginBootstrapper
    {
        [BootstrapperService(ServiceType = typeof(IConfigurationService), BridgeType = typeof(ConfigurationService))]
        private readonly IConfigurationService configurationService;

        public IServiceCollection Bootstrap(IServiceCollection services)
        {
            services
                .AddScoped<TableConfig>((serviceProvider) =>
                {
                    var connectionString = this.configurationService.GetConfigurationValueForKey("TableStorage:ConnectionString");
                    var tableName = this.configurationService.GetConfigurationValueForKey("TableStorage:TableName");

                    // Set the TableStorageProviderBase.config variable
                    return new TableConfig
                    {
                        ConnectionString = connectionString,
                        TableName = tableName
                    };
                })
                .AddScoped<ITableStorageService>(serviceProvider =>
                {
                    var config = serviceProvider.GetRequiredService<TableConfig>();

                    return new TableStorageService(config);
                });

            return services;
        }
    }
}