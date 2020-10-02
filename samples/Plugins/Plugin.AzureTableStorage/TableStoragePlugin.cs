
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Example.Contract;
using Prise.Plugin;

namespace PluginAzureTableStorage
{
    [Plugin(PluginType = typeof(IPlugin))]
    public class TableStoragePlugin : TableStorageProviderBase<DataEntity>, IPlugin
    {
        [PluginService(ProvidedBy = ProvidedBy.Host, ServiceType = typeof(IConfigurationService), BridgeType = typeof(ConfigurationService))]
        private readonly IConfigurationService configurationService;

        [PluginActivated]
        public void OnActivated()
        {
            var connectionString = this.configurationService.GetConfigurationValueForKey("TableStorage:ConnectionString");
            var tableName = this.configurationService.GetConfigurationValueForKey("TableStorage:TableName");

            // Set the TableStorageProviderBase.config variable
            this.config = new TableConfig
            {
                ConnectionString = connectionString,
                TableName = tableName
            };
        }

        public async Task<MyDto> Create(int number, string text)
        {
            await this.ConnectToTableAsync();
            return ToDto(await this.InsertOrUpdate(new DataEntity
            {
                Number = number,
                Text = text
            }));
        }

        public async Task<IEnumerable<MyDto>> GetAll()
        {
            await this.ConnectToTableAsync();
            var tableEntities = await this.GetAllItems();
            return tableEntities.Select(e => ToDto(e));
        }

        private MyDto ToDto(DataEntity e) => new MyDto
        {
            Number = e.Number,
            Text = e.Text
        };
    }
}