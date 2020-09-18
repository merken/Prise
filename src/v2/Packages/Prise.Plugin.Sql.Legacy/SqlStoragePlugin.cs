using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Prise.Console.Contract;
using Microsoft.EntityFrameworkCore;
using Prise.Plugin.Sql.Legacy.Configuration;
using System.Data.SqlClient;

namespace Prise.Plugin.Sql.Legacy
{
    [Plugin(PluginType = typeof(IStoragePlugin))]
    public class SqlStoragePlugin : IStoragePlugin
    {
        private SqlDbContext dbContext;

        [PluginService(ProvidedBy = ProvidedBy.Host, ServiceType = typeof(IConfigurationService), BridgeType = typeof(ConfigurationService))]
        private readonly IConfigurationService configurationService;

        [PluginActivated]
        public void OnActivated()
        {
            var connectionString = this.configurationService.GetConfigurationValueForKey("SQL:ConnectionString");
            var connection = new SqlConnection(connectionString);
            connection.Open();

            var options = new DbContextOptionsBuilder<SqlDbContext>()
                    .UseSqlServer(connection)
                    .Options;

            this.dbContext = new SqlDbContext(options);
        }

        public async Task<PluginObject> GetData(PluginObject input)
        {
            var firstResult = await dbContext.Data
                .AsNoTracking()
                .FirstOrDefaultAsync();
            firstResult.Text += " Modified";
            return firstResult;
        }
    }
}
