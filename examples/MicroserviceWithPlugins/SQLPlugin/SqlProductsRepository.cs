using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contract;
using Microsoft.EntityFrameworkCore;
using Prise.Infrastructure;

namespace SQLPlugin
{
    [Plugin(PluginType = typeof(IProductsRepository))]
    public class SqlProductsRepository : IProductsRepository
    {
        private readonly ProductsDbContext dbContext;
        private readonly IServiceProvider serviceProvider;

        internal SqlProductsRepository(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [PluginFactory]
        public static SqlProductsRepository ThisIsTheFactoryMethod(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService(typeof(ProductsDbContext));
            return new SqlProductsRepository(serviceProvider);
        }

        public async Task<Product> Create(Product product)
        {
            await this.dbContext.Products.AddAsync(product);
            return product;
        }

        public async Task Delete(int productId)
        {
            var product = new Product { Id = productId };
            this.dbContext.Products.Attach(product);
            this.dbContext.Products.Remove(product);
            await this.dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Product>> All()
        {
            var dbContext = (ProductsDbContext)serviceProvider.GetService(typeof(ProductsDbContext));
            return await dbContext.Products
                .AsNoTracking()
                .ToListAsync();
        }

        public Task<Product> Get(int productId)
        {
            return this.dbContext.Products
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == productId);
        }

        public async Task<IEnumerable<Product>> Search(string term)
        {
            var dbContext = (ProductsDbContext)serviceProvider.GetService(typeof(ProductsDbContext));
            return await dbContext.Products
                .AsNoTracking()
                .Where(p => p.Description.Contains(term))
                .ToListAsync();
        }

        public async Task<Product> Update(Product product)
        {
            this.dbContext.Products.Attach(product);
            await this.dbContext.SaveChangesAsync();
            return product;
        }
    }
}
