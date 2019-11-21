using Contract;
using Prise.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpPlugin
{
    [Plugin(PluginType = typeof(IProductsRepository))]
    public class SWAPIRepository : HttpRepositoryBase, IProductsRepository
    {
        internal SWAPIRepository(HttpClient client, HttpConfig config)
            : base(client, new Uri(config.SWAPIUrl))
        {
        }

        [PluginFactory]
        public static SWAPIRepository SWAPIRepositoryPluginFactory(IServiceProvider serviceProvider) =>
           new SWAPIRepository(
               (HttpClient)serviceProvider.GetService(typeof(HttpClient)),
               (HttpConfig)serviceProvider.GetService(typeof(HttpConfig)));

        public async Task<IEnumerable<Product>> All()
        {
            var results = await SendAync<SWAPIFilmsResponse>(HttpMethod.Get, "/api/films/");
            return results.Films.Select(f => ToProduct(f));
        }

        public Task<Product> Create(Product product)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public Task<Product> Update(Product product)
        {
            throw new NotImplementedException();
        }

        private Product ToProduct(SWAPIFilm film)
        {
            return new Product
            {
                Id = film.EpisodeId,
                Name = film.Title,
                Description = film.Description,
                SKU = film.Director
            };
        }
    }
}
