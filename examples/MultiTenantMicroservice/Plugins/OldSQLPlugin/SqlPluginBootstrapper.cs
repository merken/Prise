using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OldSQLPlugin.Configuration;
using Prise.Plugin;
using System.Data.Common;
using System.Data.SqlClient;

namespace OldSQLPlugin
{
    [PluginBootstrapper(PluginType = typeof(SqlProductsRepository))]
    public class SqlPluginBootstrapper : IPluginBootstrapper
    {
        public IServiceCollection Bootstrap(IServiceCollection services)
        {
            var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            var sqlConfig = new SQLPluginConfig();
            config.Bind("SQLPlugin", sqlConfig);

            services.AddScoped<DbConnection>((serviceProvider) =>
            {
                // using System.Data.SqlClient
                var dbConnection = new SqlConnection(sqlConfig.ConnectionString);
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
