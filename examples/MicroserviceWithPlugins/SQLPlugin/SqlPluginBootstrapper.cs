using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prise.Infrastructure;

namespace SQLPlugin
{
    [PluginBootstrapper(PluginType = typeof(SqlProductsRepository))]
    public class SqlPluginBootstrapper : IPluginBootstrapper
    {
        public IServiceCollection Bootstrap(IServiceCollection services)
        {
            var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("ProductsDb");

            services.AddScoped<DbConnection>((serviceProvider) =>
            {
                var dbConnection = new SqlConnection(connectionString);
                dbConnection.Open();
                return dbConnection;
            });

            services.AddScoped<DbContextOptions>((serviceProvider) =>
            {
                var dbConnection = serviceProvider.GetService<DbConnection>();
                return new DbContextOptionsBuilder<ProductsDbContext>()
                    .UseSqlServer(dbConnection)
                    .Options;
            });

            services.AddScoped<ProductsDbContext>((serviceProvider) =>
            {
                var options = serviceProvider.GetService<DbContextOptions>();
                var context = new ProductsDbContext(options);
                return context;
            });

            return services;
        }
    }
}