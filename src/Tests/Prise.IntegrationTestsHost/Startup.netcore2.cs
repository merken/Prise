using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;

using Prise;
using Prise.IntegrationTestsContract;
using Prise.IntegrationTestsHost.Services;
using Prise.IntegrationTestsHost.Custom;
using System;

namespace Prise.IntegrationTestsHost
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
