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
#if NETCORE2_1
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((c, b) => b.AddInMemoryCollection(this.settings));
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<ICommandLineArguments>((s) => this.commandLineArguments);
            });
        }
#endif
    }
}