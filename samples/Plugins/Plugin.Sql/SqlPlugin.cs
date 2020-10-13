using System;
using Prise.Plugin;
using System.Collections.Generic;
using System.Threading.Tasks;
using Example.Contract;
using Microsoft.EntityFrameworkCore;

namespace Plugin.Sql
{
    [Plugin(PluginType = typeof(IPlugin))]
    public class SqlPlugin // I do not necessarily need to implement the IPlugin interface, I just need to make sure my methods respect the Contract
    {
        [PluginService(ProvidedBy = ProvidedBy.Plugin, ServiceType = typeof(SqlDbContext))]
        private readonly SqlDbContext dbContext;

        [PluginActivated]
        public void OnActivated()
        {
            // TODO some activation code here
            Console.WriteLine("SqlPlugin Activated!");
        }

        public async Task<IEnumerable<MyDto>> GetAll()
        {
            return await dbContext.Data
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
