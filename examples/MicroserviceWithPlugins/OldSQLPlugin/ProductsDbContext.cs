using Contract;
using Microsoft.EntityFrameworkCore;
using System;

namespace OldSQLPlugin
{
    public class ProductsDbContext : DbContext
    {
        public virtual DbSet<Product> Products { get; set; }

        public ProductsDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
