using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyHost.Infrastructure;

namespace Tests
{
#if NETCORE2_1
    public class MyHost2WebApplicationFactory
       : WebApplicationFactory<MyHost2.Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
            .ConfigureAppConfiguration((context, config) =>
            {
                // Always add the dev file
                config.AddJsonFile("appsettings.Development.json", true, true);
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton<ICommandLineArguments>(new UseLocalCommandLineArguments());
            });
        }
    }
#endif
}