using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prise.Plugin;
using SQLPlugin.Configuration;

namespace SQLPlugin
{
    [Plugin(PluginType = typeof(IProductsRepository))]
    public class SqlProductsRepository : IProductsRepository
    {
        private readonly ProductsDbContext dbContext;
        internal SqlProductsRepository(ProductsDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public SqlProductsRepository(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var sqlConfig = new SQLPluginConfig();
            config.Bind("SQLPlugin", sqlConfig);
            var options = new DbContextOptionsBuilder<ProductsDbContext>()
                    .UseSqlServer(sqlConfig.ConnectionString)
                    .Options;
            this.dbContext = new ProductsDbContext(options);
        }

        [PluginFactory]
        public static SqlProductsRepository ThisIsTheFactoryMethod(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService(typeof(ProductsDbContext));
            return new SqlProductsRepository(service as ProductsDbContext);
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
