using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Contract
{
    public interface IProductsWriter
    {
        Task<Product> Create(Product product);
        Task<Product> Update(Product product);
    }
}
