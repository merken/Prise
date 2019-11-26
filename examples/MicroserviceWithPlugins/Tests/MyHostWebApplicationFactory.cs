using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyHost.Infrastructure;

namespace Tests
{
#if NETCORE3_0
    public class MyHostWebApplicationFactory
       : WebApplicationFactory<MyHost.Startup>
    {
        protected override IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Always add the dev file
                    config.AddJsonFile("appsettings.Development.json", true, true);
                })
                .ConfigureServices((hostContext, services) =>
                    services.AddSingleton<ICommandLineArguments>(new UseLocalCommandLineArguments()))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<MyHost.Startup>();
                });
        }
    }
#endif
}