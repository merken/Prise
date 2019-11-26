using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyHost.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MyHost2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var consoleConfig = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Always add the dev file
                    config.AddJsonFile("appsettings.Development.json", true, true);
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ICommandLineArguments>(new CommandLineArguments(consoleConfig));
                })
                .UseStartup<Startup>();
        }
    }
}
