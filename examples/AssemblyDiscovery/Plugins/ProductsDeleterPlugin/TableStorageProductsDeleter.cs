using Contract;
using Prise.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableStorageConnector;

namespace ProductsDeleterPlugin
{
    [Plugin(PluginType = typeof(IProductsDeleter))]
    public class TableStorageProductsDeleter : TableStorageConnector<Product>, IProductsDeleter
    {
        internal TableStorageProductsDeleter(
            TableStorageConfig config)
            : base(config,
                  (p) => p.Id.ToString(),
                  (p, value) => p.Id = int.Parse(value),
                  (p) => p.Name,
                  (p, value) => p.Name = value)
        {
        }

        [PluginFactory]
        public static TableStorageProductsDeleter ThisIsTheFactoryMethod(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService(typeof(TableStorageConfig));
            return new TableStorageProductsDeleter(config as TableStorageConfig);
        }

        public async Task Delete(int productId)
        {
            var items = await base.Search($"Id eq {productId}");
            var item = items.First();
            await base.Delete(item.PartitionKey, item.RowKey);
        }
    }
}
