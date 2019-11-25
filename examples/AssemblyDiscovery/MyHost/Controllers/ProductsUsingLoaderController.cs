using Contract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prise.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsUsingLoaderController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly IPluginLoader<IProductsReader> readerLoader;
        private readonly IPluginLoader<IProductsWriter> writerLoader;
        private readonly IPluginLoader<IProductsDeleter> deleterLoader;

        public ProductsUsingLoaderController(ILogger<ProductsController> logger,
            IPluginLoader<IProductsReader> readerLoader,
            IPluginLoader<IProductsWriter> writerLoader,
            IPluginLoader<IProductsDeleter> deleterLoader)
        {
            _logger = logger;
            this.readerLoader = readerLoader;
            this.writerLoader = writerLoader;
            this.deleterLoader = deleterLoader;
        }

        [HttpGet]
        public async Task<IEnumerable<Product>> Get([FromQuery] string searchTerm)
        {
            var repository = await readerLoader.Load();
            if (String.IsNullOrEmpty(searchTerm))
                return await repository.All();

            return await repository.Search(searchTerm);
        }

        [HttpGet("{productId}")]
        public async Task<Product> Get(int productId)
        {
            var repository = await readerLoader.Load();
            return await repository.Get(productId);
        }

        [HttpPost]
        public async Task<Product> Create(Product product)
        {
            var repository = await writerLoader.Load();
            return await repository.Create(product);
        }

        [HttpDelete("{productId}")]
        public async Task Delete(int productId)
        {
            var repository = await deleterLoader.Load();
            await repository.Delete(productId);
        }

        [HttpPut]
        public async Task<Product> Update(Product product)
        {
            var repository = await writerLoader.Load();
            return await repository.Update(product);
        }
    }
}
