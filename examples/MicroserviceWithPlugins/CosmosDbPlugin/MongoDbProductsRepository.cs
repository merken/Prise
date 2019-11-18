using Contract;
using Prise.Plugin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CosmosDbPlugin
{
    [Plugin(PluginType = typeof(IProductsRepository))]
    public class MongoDbProductsRepository : MongoDbRepositoryBase<Product>, IProductsRepository
    {
        private bool disposed = false;
        internal MongoDbProductsRepository(MongoDbConfig config) : base(config, "products") { }

        [PluginFactory]
        public static MongoDbProductsRepository ThisIsTheFactoryMethod(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService(typeof(MongoDbConfig));
            return new MongoDbProductsRepository(config as MongoDbConfig);
        }

        public Task<IEnumerable<Product>> All()
        {
            return this.GetAll();
        }

        public Task<Product> Create(Product product)
        {
            return Insert(product);
        }

        public Task Delete(int productId)
        {
            return Delete(productId.ToString());
        }

        public Task<Product> Get(int productId)
        {
            return GetItem(productId.ToString());
        }

        public Task<IEnumerable<Product>> Search(string term)
        {
            return base.Search(term);
        }

        public Task<Product> Update(Product product)
        {
            return base.Update(product.Id.ToString(), product);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                //
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
