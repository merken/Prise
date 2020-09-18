using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Prise.Console.Contract;
using Microsoft.EntityFrameworkCore;
using Prise.Console.Contract;

namespace Prise.Plugin.Sql.Legacy
{
    [Plugin(PluginType = typeof(IStoragePlugin))]
    public class SqlProductsRepository : IStoragePlugin
    {
        private readonly SqlDbContext dbContext;
        internal SqlProductsRepository(SqlDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [PluginFactory]
        public static SqlProductsRepository ThisIsTheFactoryMethod(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService(typeof(SqlDbContext));
            return new SqlProductsRepository(service as SqlDbContext);
        }

        public async Task<PluginObject> GetData(PluginObject input)
        {
            return await dbContext.Data
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
    }
}
