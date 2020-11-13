using System;
using Prise.Plugin;
using System.Collections.Generic;
using System.Threading.Tasks;
using Example.Contract;
using Microsoft.EntityFrameworkCore;

namespace Plugin.SqlClient
{
    [Plugin(PluginType = typeof(IPlugin))]
    public class SqlClientPlugin // I do not necessarily need to implement the IPlugin interface, I just need to make sure my methods respect the Contract
    {
        [PluginService(ProvidedBy = ProvidedBy.Plugin, ServiceType = typeof(SqlDbContext))]
        private readonly SqlDbContext dbContext;

        [PluginActivated]
        public void OnActivated()
        {
            // TODO some activation code here
            Console.WriteLine("SqlClientPlugin Activated!");
        }

        public async Task<IEnumerable<MyDto>> GetAll()
        {
            return await dbContext.Data
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
