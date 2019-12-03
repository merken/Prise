using Contract;
using Microsoft.EntityFrameworkCore;
using Prise.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OldSQLPlugin
{
    [Plugin(PluginType = typeof(IProductsRepository))]
    public class SqlProductsRepository : IProductsRepository
    {
        private readonly ProductsDbContext dbContext;
        internal SqlProductsRepository(ProductsDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [PluginFactory]
        public static SqlProductsRepository ThisIsTheFactoryMethod(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService(typeof(ProductsDbContext));
            return new SqlProductsRepository(service as ProductsDbContext);
        }

        public async Task<IEnumerable<Product>> All()
        {
            return await dbContext.Products
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
