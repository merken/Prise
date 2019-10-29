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

namespace AppHost
{
    public partial class Program
    {

#if NETCORE2_1
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) 
        {
            var consoleConfig = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            return WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(services => 
                {
                    services.AddSingleton<ICommandLineArguments>(new CommandLineArguments(consoleConfig));
                })
                .UseStartup<Startup>();
        }
#endif
    }
}
