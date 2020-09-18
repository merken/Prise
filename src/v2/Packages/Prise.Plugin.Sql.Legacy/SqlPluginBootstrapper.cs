using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prise.Plugin.Sql.Legacy.Configuration;
using Prise.Plugin;
using System.Data.Common;
using System.Data.SqlClient;

namespace Prise.Plugin.Sql.Legacy
{
    // [PluginBootstrapper(PluginType = typeof(SqlStoragePlugin))]
    public class SqlStoragePluginBoostrapper : IPluginBootstrapper
    {
        public IServiceCollection Bootstrap(IServiceCollection services)
        {
            services.AddScoped<DbConnection>((serviceProvider) =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                var sqlConfig = new SQLPluginConfig();
                config.Bind("SQLPlugin", sqlConfig);

                // using System.Data.SqlClient
                var dbConnection = new SqlConnection(sqlConfig.ConnectionString);
                dbConnection.Open();
                return dbConnection;
            });

            services.AddScoped<DbContextOptions>((serviceProvider) =>
            {
                var dbConnection = serviceProvider.GetService<DbConnection>();
                return new DbContextOptionsBuilder<SqlDbContext>()
                    .UseSqlServer(dbConnection)
                    .Options;
            });

            services.AddScoped<SqlDbContext>((serviceProvider) =>
            {
                var options = serviceProvider.GetService<DbContextOptions>();
                var context = new SqlDbContext(options);
                return context;
            });

            return services;
        }
    }
}
