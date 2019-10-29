using AppHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tests
{
    public partial class AppHostWebApplicationFactory
       : WebApplicationFactory<AppHost.Startup>
    {
#if NETCORE2_1
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<ICommandLineArguments>(new CommandLineArgumentsLazy());
            });
        }
#endif
    }
}