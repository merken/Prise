using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Prise.IntegrationTestsHost
{
    public partial class Program
    {
#if NETCORE3_0 || NETCORE3_1
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var consoleConfig = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                    services.AddSingleton<ICommandLineArguments>(new CommandLineArguments(consoleConfig)))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseUrls("http://localhost:5003");
                });
        }
#endif
    }
}
