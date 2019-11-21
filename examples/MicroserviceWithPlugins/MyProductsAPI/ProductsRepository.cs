using Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyProductsAPI
{
    public class ProductsRepository : IProductsRepository
    {
        private readonly List<Product> inMemoryProducts;

        public ProductsRepository()
        {
            this.inMemoryProducts = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "My Product 1",
                    Description = "My Product 1 description",
                    SKU = "products",
                    PriceExlVAT = 100.55m,
                },
                new Product
                {
                    Id = 2,
                    Name = "My Product 2",
                    Description = "My Product 2 description",
                    SKU = "products",
                    PriceExlVAT = 99.99m,
                }
            };
        }

        public Task<IEnumerable<Product>> All()
        {
            return Task.FromResult(inMemoryProducts.AsEnumerable());
        }

        public Task<Product> Create(Product product)
        {
            this.inMemoryProducts.Add(product);
            return Task.FromResult(product);
        }

        public Task Delete(int productId)
        {
            this.inMemoryProducts.Remove(this.inMemoryProducts.First(p => p.Id == productId));
            return Task.CompletedTask;
        }

        public Task<Product> Get(int productId)
        {
            return Task.FromResult(this.inMemoryProducts.First(p => p.Id == productId));
        }

        public Task<IEnumerable<Product>> Search(string term)
        {
            return Task.FromResult(inMemoryProducts.Where(p => p.Name.Contains(term)).AsEnumerable());
        }

        public Task<Product> Update(Product product)
        {
            this.inMemoryProducts.Remove(this.inMemoryProducts.First(p => p.Id == product.Id));
            this.inMemoryProducts.Add(product);
            return Task.FromResult(this.inMemoryProducts.First(p => p.Id == product.Id));
        }
    }
}
