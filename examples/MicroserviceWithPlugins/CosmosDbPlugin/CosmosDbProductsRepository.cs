using Contract;
using Prise.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDbPlugin
{
    [Plugin(PluginType = typeof(IProductsRepository))]
    public class CosmosDbProductsRepository : CosmosDbRepositoryBase, IProductsRepository
    {
        internal CosmosDbProductsRepository(CosmosDbConfig config) : base(config) { }

        [PluginFactory]
        public static CosmosDbProductsRepository ThisIsTheFactoryMethod(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService(typeof(CosmosDbConfig));
            return new CosmosDbProductsRepository(config as CosmosDbConfig);
        }

        public async Task<IEnumerable<Product>> All()
        {
            await InitializeAsync();
            return (await GetItemsAsync()).Select(d => ToProduct(d));

        }

        public async Task<Product> Create(Product product)
        {
            await InitializeAsync();
            await AddItemAsync(ToDocument(product));
            return product;

        }

        public async Task Delete(int productId)
        {
            await InitializeAsync();
            await DeleteItemAsync(productId.ToString());
        }

        public async Task<Product> Get(int productId)
        {
            await InitializeAsync();
            return ToProduct(await GetItemAsync(productId.ToString()));
        }

        public async Task<IEnumerable<Product>> Search(string term)
        {
            await InitializeAsync();
            return (await GetItemsAsync(term)).Select(d => ToProduct(d));
        }

        public async Task<Product> Update(Product product)
        {
            await InitializeAsync();
            await UpdateItemAsync(product.Id.ToString(), ToDocument(product));
            return product;

        }

        private Product ToProduct(ProductDocument d) => new Product
        {
            Id = d.Id,
            Name = d.Name,
            SKU = d.SKU,
            Description = d.Description,
            PriceExlVAT = d.PriceExlVAT,
        };

        private ProductDocument ToDocument(Product p) => new ProductDocument
        {
            Id = p.Id,
            Name = p.Name,
            SKU = p.SKU,
            Description = p.Description,
            PriceExlVAT = p.PriceExlVAT,
        };
    }
}
