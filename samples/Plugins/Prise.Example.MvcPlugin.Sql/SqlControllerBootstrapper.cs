using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prise.Plugin;
using Prise.Example.Contract;

namespace Prise.Example.MvcPlugin.Sql
{
    [PluginBootstrapper(PluginType = typeof(SqlController))]
    public class SqlControllerBootstrapper : IPluginBootstrapper
    {
        [BootstrapperService(ServiceType = typeof(IConfigurationService), BridgeType = typeof(ConfigurationService))]
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
