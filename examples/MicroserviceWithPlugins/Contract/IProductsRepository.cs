using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contract
{
    public interface IProductsRepository //: IDisposable
    {
        Task<Product> Get(int productId);
        Task<Product> Create(Product product);
        Task<Product> Update(Product product);
        Task Delete(int productId);
        Task<IEnumerable<Product>> All();
        Task<IEnumerable<Product>> Search(string term);
    }
}
