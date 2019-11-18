using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;

using Prise;
using Contract;
using AppHost.Services;
using AppHost.Custom;
using System;

namespace AppHost
{
    public partial class Startup
    {
#if NETCORE2_1
        private void ConfigureTargetFramework(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }
#endif

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
#if NETCORE2_1
        private void ConfigureTargetFramework(IApplicationBuilder app)
        {
            app.UseMvc();
        }
#endif
    }
}
