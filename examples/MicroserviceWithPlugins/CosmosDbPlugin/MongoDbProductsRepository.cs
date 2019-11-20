using Contract;
using MongoDB.Bson;
using Prise.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CosmosDbPlugin
{
    [Plugin(PluginType = typeof(IProductsRepository))]
    public class MongoDbProductsRepository : MongoDbRepositoryBase<ProductMongoDbDocument>, IProductsRepository
    {
        private bool disposed = false;
        internal MongoDbProductsRepository(MongoDbConfig config) : base(config, "products") { }

        [PluginFactory]
        public static MongoDbProductsRepository ThisIsTheFactoryMethod(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService(typeof(MongoDbConfig));
            return new MongoDbProductsRepository(config as MongoDbConfig);
        }

        public async Task<IEnumerable<Product>> All()
        {
            return (await this.GetAll()).Select(p => ToProduct(p));
        }

        public async Task<Product> Create(Product product)
        {
            var doc = ToDocument(product);
            doc.InternalId = ObjectId.GenerateNewId();
            return ToProduct((await Insert(ToDocument(product))));
        }

        public Task Delete(int productId)
        {
            return Delete(productId.ToString());
        }

        public async Task<Product> Get(int productId)
        {
            return ToProduct((await GetItem(productId.ToString())));
        }

        public async Task<IEnumerable<Product>> Search(string term)
        {
            return (await base.Search(d => d.Name.Contains(term))).Select(p => ToProduct(p));
        }

        public async Task<Product> Update(Product product)
        {
            return ToProduct((await base.Update(product.Id.ToString(), ToDocument(product))));
        }

        private Product ToProduct(ProductMongoDbDocument d) => new Product
        {
            Id = int.Parse(d.Id),
            Name = d.Name,
            SKU = d.SKU,
            Description = d.Description,
            PriceExlVAT = d.PriceExlVAT,
        };

        private ProductMongoDbDocument ToDocument(Product p) => new ProductMongoDbDocument
        {
            Id = p.Id.ToString(),
            Name = p.Name,
            SKU = p.SKU,
            Description = p.Description,
            PriceExlVAT = p.PriceExlVAT,
        };
    }
}
