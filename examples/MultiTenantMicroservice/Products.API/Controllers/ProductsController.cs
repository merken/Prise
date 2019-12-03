using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Products.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> logger;
        private readonly IProductsRepository productsRepository;

        public ProductsController(ILogger<ProductsController> logger, IProductsRepository productsRepository)
        {
            this.logger = logger;
            this.productsRepository = productsRepository;
        }

        [HttpGet]
        public Task<IEnumerable<Product>> Get()
        {
            return this.productsRepository.All();
        }

        [HttpGet("{id}")]
        public Task<Product> Get(string id)
        {
            return this.productsRepository.Get(int.Parse(id));
        }
    }
}
