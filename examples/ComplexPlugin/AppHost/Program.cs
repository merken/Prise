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
        public static void Main(string[] args)
        {
#if NETCORE3_0
            CreateHostBuilder(args).Build().Run();

#endif
#if NETCORE2_1
            CreateWebHostBuilder(args).Build().Run();
#endif
        }
    }
}
