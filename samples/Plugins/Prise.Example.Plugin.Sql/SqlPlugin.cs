using System;
using Prise.Plugin;
using System.Collections.Generic;
using System.Threading.Tasks;
using Prise.Example.Contract;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;

namespace Prise.Example.Plugin.Sql
{
    [Plugin(PluginType = typeof(IPlugin))]
    public class SqlPlugin // I do not necessarily need to implement the IPlugin interface, I just need to make sure my methods respect the Contract
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

        public async Task<IEnumerable<MyDto>> GetAll()
        {
            return await dbContext.Data
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
