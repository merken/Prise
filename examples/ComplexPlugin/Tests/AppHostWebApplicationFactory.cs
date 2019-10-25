using AppHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tests
{
    public class CommandLineArgumentsLazy : ICommandLineArguments
    {
        public bool UseLazyService => true;
    }

    public class AppHostWebApplicationFactory
       : WebApplicationFactory<AppHost.Startup>
    {
        private bool useLazyServices = false;
        public void ConfigureLazyService()
        {
            this.useLazyServices = true;
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                    services.AddSingleton<ICommandLineArguments>(new CommandLineArgumentsLazy()))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<AppHost.Startup>();
                });
        }
    }
}