using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;

using Prise.DependencyInjection;
using Prise.IntegrationTestsContract;
using Prise.IntegrationTestsHost.Services;
using System;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Prise.IntegrationTestsHost.Controllers;
using Prise.IntegrationTestsHost.PluginLoaders;
using System.Reflection;
using System.Runtime.Versioning;

namespace Prise.IntegrationTestsHost
{
    public partial class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureTargetFramework(services);

            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddTransient<IHostFrameworkProvider, AppHostFrameworkProvider>();
            services.AddTransient<IPluginLoader, PluginLoader>();
            services.AddPrise();

            AddPriseCalculationPlugins(services);
        }

        protected virtual IServiceCollection AddPriseCalculationPlugins(IServiceCollection services)
        {
            return services.AddTransient<ICalculationPluginLoader, CalculationPluginLoader>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
#if NETCORE3_0 || NETCORE3_1
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
#endif
#if NETCORE2_1
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
#endif
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            ConfigureTargetFramework(app);

            // var provider = new FileExtensionContentTypeProvider();
            // // Add new mappings
            // provider.Mappings[".dll"] = "application/x-msdownload";
            // app.UseStaticFiles(new StaticFileOptions
            // {
            //     ServeUnknownFileTypes = true,
            //     FileProvider = new PhysicalFileProvider(
            //         Path.Combine(Directory.GetCurrentDirectory(), "Plugins")),
            //     RequestPath = "/Plugins",
            //     ContentTypeProvider = provider
            // });

            // app.UseDirectoryBrowser(new DirectoryBrowserOptions
            // {
            //     FileProvider = new PhysicalFileProvider(
            //         Path.Combine(Directory.GetCurrentDirectory(), "Plugins")),
            //     RequestPath = "/Plugins"
            // });
        }
    }
}
