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
        private readonly IProductsReader reader;
        private readonly IProductsWriter writer;
        private readonly IProductsDeleter deleter;

        public ProductsController(
            ILogger<ProductsController> logger,
            IProductsReader reader,
            IProductsWriter writer,
            IProductsDeleter deleter)
        {
            this.reader = reader;
            this.writer = writer;
            this.deleter = deleter;
            _logger = logger;
        }

        [HttpGet]
        public Task<IEnumerable<Product>> Get([FromQuery] string searchTerm)
        {
            if (String.IsNullOrEmpty(searchTerm))
                return reader.All();

            return reader.Search(searchTerm);
        }

        [HttpGet("{productId}")]
        public Task<Product> Get(int productId)
        {
            return this.reader.Get(productId);
        }

        [HttpPost]
        public Task<Product> Create(Product product)
        {
            return this.writer.Create(product);
        }

        [HttpDelete("{productId}")]
        public async Task Delete(int productId)
        {
            await this.deleter.Delete(productId);
        }

        [HttpPut]
        public Task<Product> Update(Product product)
        {
            return this.writer.Update(product);
        }
    }
}
