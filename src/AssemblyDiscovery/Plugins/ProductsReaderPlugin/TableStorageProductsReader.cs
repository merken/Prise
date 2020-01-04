using Contract;
using Prise.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableStorageConnector;

namespace ProductsReaderPlugin
{
    [Plugin(PluginType = typeof(IProductsReader))]
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

        public async Task<IEnumerable<Product>> All()
        {
            return (await base.GetAll()).Select(e => e.Value);
        }

        public async Task<Product> Get(int productId)
        {
            var items = await base.Search($"id={productId}");
            return items.Select(e => e.Value).FirstOrDefault();
        }

        public async Task<IEnumerable<Product>> Search(string term)
        {
            return (await base.Search(term)).Select(e => e.Value);
        }
    }
}
