using System.Linq;
using System.Threading.Tasks;
using Prise.Console.Contract;
using Prise.Plugin.Azure.TableStorage.Configuration;

namespace Prise.Plugin.Azure.TableStorage
{
    [Plugin(PluginType = typeof(IStoragePlugin))]
    public class TableStoragePlugin : TableStorageProviderBase<DataEntity>, IStoragePlugin
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

        public async Task<PluginObject> GetData(PluginObject input)
        {
            await ConnectToTableAsync();
            var tableEntities = await GetAll();
            return tableEntities.Select(e => ToPluginObject(e)).FirstOrDefault();
        }

        private PluginObject ToPluginObject(DataEntity e) => new PluginObject
        {
            Number = e.Number,
            Text = e.Text
        };
    }
}