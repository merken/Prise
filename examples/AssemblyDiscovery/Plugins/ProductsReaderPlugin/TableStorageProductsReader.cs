using Contract;
using Prise.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableStorageConnector;

namespace ProductsReaderPlugin
{
    public class TableStorageProductsReader : TableStorageConnector<Product>, IProductsReader
    {
        internal TableStorageProductsReader(
            TableStorageConfig config)
            : base(config,
                  (p) => p.Id.ToString(),
                  (p, value) => p.Id = int.Parse(value),
                  (p) => p.Name,
                  (p, value) => p.Name = value)
        {
        }

        [PluginFactory]
        public static TableStorageProductsReader ThisIsTheFactoryMethod(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService(typeof(TableStorageConfig));
            return new TableStorageProductsReader(config as TableStorageConfig);
        }

        public Task<IEnumerable<Product>> All()
        {
            return base.GetAll();
        }

        public async Task<Product> Get(int productId)
        {
            var items = await base.Search($"id={productId}");
            return items.FirstOrDefault();
        }

        public Task<IEnumerable<Product>> Search(string term)
        {
            return base.Search(term);
        }
    }
}
