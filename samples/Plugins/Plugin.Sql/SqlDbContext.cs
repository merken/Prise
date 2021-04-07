using Example.Contract;
using Microsoft.EntityFrameworkCore;

namespace Plugin.Sql
{
    public class SqlDbContext : DbContext
    {
        public virtual DbSet<MyDto> Data { get; set; }

        public SqlDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyDto>()
                .HasKey(p => p.Number); // No need to alter the Contract, I will annotate the PK myself using EF Core
        }
    }
}