using Contract;
using Prise.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TableStoragePlugin
{
    [Plugin(PluginType = typeof(IProductsRepository))]
    public class TableStorageProductsRepository : TableStorageProviderBase<ProductTableEntity>, IProductsRepository
    {
        internal TableStorageProductsRepository(TableStorageConfig config) : base(config) { }

        [PluginFactory]
        public static TableStorageProductsRepository ThisIsTheFactoryMethod(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService(typeof(TableStorageConfig));
            return new TableStorageProductsRepository(config as TableStorageConfig);
        }

        public async Task<IEnumerable<Product>> All()
        {
            await ConnectToTableAsync();
            var tableEntities = await GetAll();
            return tableEntities.Select(e => ToProduct(e));
        }

        public async Task<Product> Create(Product product)
        {
            await ConnectToTableAsync();
            var productEntity = new ProductTableEntity
            {
                PartitionKey = product.Id.ToString(),
                RowKey = product.Name,
                Id = product.Id,
                Description = product.Description,
                Name = product.Name,
                SKU = product.SKU,
                PriceExlVAT = product.PriceExlVAT,
                Timestamp = DateTimeOffset.Now
            };
            return ToProduct(await InsertOrUpdate(productEntity));
        }

        public async Task Delete(int productId)
        {
            await ConnectToTableAsync();
            var product = await SingleEntityById(productId);
            await Delete(product.PartitionKey, product.RowKey);
        }

        public async Task<Product> Get(int productId)
        {
            await ConnectToTableAsync();
            var product = await SingleEntityById(productId);
            return ToProduct(product);
        }

        public async Task<IEnumerable<Product>> Search(string term)
        {
            await ConnectToTableAsync();
            var results = await base.Search(term);
            return results.Select(e => ToProduct(e));
        }

        public async Task<Product> Update(Product product)
        {
            await ConnectToTableAsync();
            var productEntity = new ProductTableEntity
            {
                PartitionKey = product.Id.ToString(),
                RowKey = product.Name,
                Id = product.Id,
                Description = product.Description,
                Name = product.Name,
                SKU = product.SKU,
                PriceExlVAT = product.PriceExlVAT,
                Timestamp = DateTimeOffset.Now
            };
            return ToProduct(await InsertOrUpdate(productEntity));
        }

        private async Task<ProductTableEntity> SingleEntityById(int productId)
        {
            var products = await base.Search($"id={productId}");
            if (products.Count() == 0 || products.Count() > 1)
                throw new NotSupportedException($"Wrong number of products ({products.Count()}) found for id {productId}");
            return products.Single();
        }

        private Product ToProduct(ProductTableEntity e) => new Product
        {
            Id = e.Id,
            Name = e.Name,
            SKU = e.SKU,
            Description = e.Description,
            PriceExlVAT = e.PriceExlVAT,
        };
    }
}
