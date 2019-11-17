using Contract;
using Microsoft.EntityFrameworkCore;
namespace SQLPlugin
{
    public class ProductsDbContext : DbContext
    {
        public virtual DbSet<Product> Products { get; set; }

        public ProductsDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}