using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Contract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace MyHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly IProductsRepository repository;

        public ProductsController(
            ILogger<ProductsController> logger,
            IProductsRepository repository
            )
        {
            this.repository = repository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<Product>> Get([FromQuery] string searchTerm)
        {
            IEnumerable<Product> result = null;
            if (String.IsNullOrEmpty(searchTerm))
                result = await repository.All();
            else
                result = await repository.Search(searchTerm);

            return result;
        }

        [HttpGet("{productId}")]
        public Task<Product> Get(int productId)
        {
            return this.repository.Get(productId);
        }

        [HttpPost]
        public Task<Product> Create(Product product)
        {
            return this.repository.Create(product);
        }

        [HttpDelete("{productId}")]
        public async Task Delete(int productId)
        {
            await this.repository.Delete(productId);
        }

        [HttpPut]
        public Task<Product> Update(Product product)
        {
            return this.repository.Update(product);
        }
    }
}
