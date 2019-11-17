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
        private readonly IPluginLoader<IProductsRepository> loader;

        public ProductsUsingLoaderController(ILogger<ProductsController> logger,
            IPluginLoader<IProductsRepository> loader)
        {
            _logger = logger;
            this.loader = loader;
        }

        [HttpGet]
        public async Task<IEnumerable<Product>> Get([FromQuery] string searchTerm)
        {
            var repository = await loader.Load();
            if (String.IsNullOrEmpty(searchTerm))
                return await repository.All();

            return await repository.Search(searchTerm);
        }

        [HttpGet("{productId}")]
        public async Task<Product> Get(int productId)
        {
            var repository = await loader.Load();
            return await repository.Get(productId);
        }

        [HttpPost]
        public async Task<Product> Create(Product product)
        {
            var repository = await loader.Load();
            return await repository.Create(product);
        }

        [HttpDelete("{productId}")]
        public async Task Delete(int productId)
        {
            var repository = await loader.Load();
            await repository.Delete(productId);
        }

        [HttpPut]
        public async Task<Product> Update(Product product)
        {
            var repository = await loader.Load();
            return await repository.Update(product);
        }
    }
}
