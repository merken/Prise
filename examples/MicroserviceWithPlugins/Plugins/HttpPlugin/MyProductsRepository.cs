using Contract;
using Prise.Plugin;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpPlugin
{
    [Plugin(PluginType = typeof(IProductsRepository))]
    public class MyProductsRepository : HttpRepositoryBase, IProductsRepository
    {
        internal MyProductsRepository(HttpClient client, HttpConfig config)
            : base(client, new Uri(config.MyProductsUrl))
        {
        }

        [PluginFactory]
        public static MyProductsRepository MyProductsRepositoryPluginFactory(IServiceProvider serviceProvider) =>
           new MyProductsRepository(
               (HttpClient)serviceProvider.GetService(typeof(HttpClient)),
               (HttpConfig)serviceProvider.GetService(typeof(HttpConfig)));

        public Task<IEnumerable<Product>> All()
        {
            return SendAync<IEnumerable<Product>>(HttpMethod.Get, "/products");
        }

        public Task<Product> Create(Product product)
        {
            return SendAync<Product>(HttpMethod.Post, "/products", product);
        }

        public Task Delete(int productId)
        {
            throw new NotImplementedException();
        }

        public Task<Product> Get(int productId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Product>> Search(string term)
        {
            return SendAync<IEnumerable<Product>>(HttpMethod.Get, $"/products?&term={term}");
        }

        public Task<Product> Update(Product product)
        {
            throw new NotImplementedException();
        }
    }
}
