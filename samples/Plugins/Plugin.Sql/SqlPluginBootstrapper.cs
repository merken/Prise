using Microsoft.Extensions.DependencyInjection;
using Example.Contract;
using Prise.Plugin;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace Plugin.Sql
{
    [Prise.Plugin.PluginBootstrapper(PluginType = typeof(SqlPlugin))]
    public class SqlPluginBootstrapper : Prise.Plugin.IPluginBootstrapper
    {
        [BootstrapperService(ServiceType = typeof(IConfigurationService), ProxyType =  typeof(ConfigurationService))]
        private readonly IConfigurationService configurationService;

        public IServiceCollection Bootstrap(IServiceCollection services)
        {
            return services
                .AddSingleton<IConfigurationService>(this.configurationService) // Add the injected service as singleton
                .AddTransient<DbConnection>(sp =>
                {
                    var configurationService = sp.GetRequiredService<IConfigurationService>();
                    var connectionString = this.configurationService.GetConfigurationValueForKey("SQL:ConnectionString");
                    var connection = new SqlConnection(connectionString);
                    connection.Open();
                    return connection;
                })
                .AddTransient<DbContextOptions>(sp =>
                {
                    var connection = sp.GetRequiredService<DbConnection>();
                    return new DbContextOptionsBuilder<SqlDbContext>()
                                        .UseSqlServer(connection)
                                        .Options;
                })
                .AddTransient<SqlDbContext>(sp =>
                {
                    var options = sp.GetRequiredService<DbContextOptions>();
                    return new SqlDbContext(options);
                });
        }
    }
}