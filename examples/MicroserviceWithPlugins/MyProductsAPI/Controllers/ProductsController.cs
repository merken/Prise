using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MyProductsAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly IProductsRepository productsRepository;

        public ProductsController(ILogger<ProductsController> logger, IProductsRepository productsRepository)
        {
            _logger = logger;
            this.productsRepository = productsRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<Product>> Get([FromQuery] string term)
        {
            if (!String.IsNullOrEmpty(term))
                return await this.productsRepository.Search(term);

            return await this.productsRepository.All();
        }

        [HttpPost]
        public async Task<Product> AddProduct([FromBody] Product product)
        {
            return await this.productsRepository.Create(product);
        }
    }
}
