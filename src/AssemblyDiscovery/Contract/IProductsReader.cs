using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contract
{
    public interface IProductsReader
    {
        Task<Product> Get(int productId);
        Task<IEnumerable<Product>> All();
        Task<IEnumerable<Product>> Search(string term);
    }
}
