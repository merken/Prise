using Prise.Console.Contract;
using Microsoft.EntityFrameworkCore;
using System;

namespace Prise.Plugin.Sql.Legacy
{
    public class SqlDbContext : DbContext
    {
        public virtual DbSet<PluginObject> Data { get; set; }

        public SqlDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PluginObject>().HasKey(p => p.Number);
        }
    }
}
