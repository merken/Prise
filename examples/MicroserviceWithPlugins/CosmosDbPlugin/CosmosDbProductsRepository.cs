using Contract;
using Prise.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDbPlugin
{
    [Plugin(PluginType = typeof(IProductsRepository))]
    public class CosmosDbProductsRepository : CosmosDbRepositoryBase<ProductCosmosDbDocument>, IProductsRepository
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
            return (await GetItemsAsync()).Select(p => ToProduct(p));

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
            Expression<Func<ProductCosmosDbDocument, bool>> predicate = null;
            if (String.IsNullOrEmpty(term))
                predicate = (p) => p.Name.Contains(term);

            return (await GetItemsAsync(predicate)).Select(p => ToProduct(p));
        }

        public async Task<Product> Update(Product product)
        {
            await InitializeAsync();
            await UpdateItemAsync(product.Id.ToString(), ToDocument(product));
            return product;

        }

        private Product ToProduct(ProductCosmosDbDocument d) => new Product
        {
            Id = int.Parse(d.Id),
            Name = d.Name,
            SKU = d.SKU,
            Description = d.Description,
            PriceExlVAT = d.PriceExlVAT,
        };

        private ProductCosmosDbDocument ToDocument(Product p) => new ProductCosmosDbDocument
        {
            Id = p.Id.ToString(),
            Name = p.Name,
            SKU = p.SKU,
            Description = p.Description,
            PriceExlVAT = p.PriceExlVAT,
        };
    }
}
