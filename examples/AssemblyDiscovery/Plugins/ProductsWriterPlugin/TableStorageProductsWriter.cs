using Contract;
using Prise.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableStorageConnector;

namespace ProductsWriterPlugin
{
    [Plugin(PluginType = typeof(IProductsWriter))]
    public class TableStorageProductsWriter : TableStorageConnector<Product>, IProductsWriter
    {
        internal TableStorageProductsWriter(
            TableStorageConfig config)
            : base(config,
                  (p) => p.Id.ToString(),
                  (p, value) => p.Id = int.Parse(value),
                  (p) => p.Name,
                  (p, value) => p.Name = value)
        {
        }

        [PluginFactory]
        public static TableStorageProductsWriter ThisIsTheFactoryMethod(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService(typeof(TableStorageConfig));
            return new TableStorageProductsWriter(config as TableStorageConfig);
        }

        public async Task<Product> Create(Product product)
        {
            return (await base.InsertOrUpdate(product)).Value;
        }


        public async Task<Product> Update(Product product)
        {
            return (await base.InsertOrUpdate(product)).Value;
        }
    }
}
