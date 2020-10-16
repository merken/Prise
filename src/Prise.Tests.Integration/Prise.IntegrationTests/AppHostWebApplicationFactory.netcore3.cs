using Prise.IntegrationTestsHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace Prise.IntegrationTests
{
    public partial class AppHostWebApplicationFactory
       : WebApplicationFactory<Prise.IntegrationTestsHost.Startup>
    {
#if NETCORE3_1
        protected override IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                    services.AddSingleton<ICommandLineArguments>((s) => this.commandLineArguments))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Prise.IntegrationTestsHost.Startup>();
                    webBuilder.ConfigureAppConfiguration(builder => builder.AddInMemoryCollection(this.settings));
                });
        }
#endif
    }
}