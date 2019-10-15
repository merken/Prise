using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;

using Prise.Infrastructure.NetCore;
using PluginContract;
using Prise.Infrastructure.NetCore.Contracts;
using PluginServer.Services;
using PluginServer.Custom;

namespace PluginServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddDirectoryBrowser();
            services.AddHttpContextAccessor();

            // Uncomment either one of these service registrations
            // services.AddScoped<ICalculationService, EagerCalculationService>();
            services.AddScoped<ICalculationService, LazyCalculationService>();

            // AddPriseWithDefaultOptions(services);
            AddPriseWithCustomerLoader(services);
        }

        private IServiceCollection AddPriseWithDefaultOptions(IServiceCollection services)
        {
            // This will look for 1 plugin in the Plugins directory
            return services.AddPrise<ICalculationPlugin>(options =>
                options
                    .WithDefaultOptions($"{Env.ContentRootPath}\\PluginServer")
                    .WithPluginAssemblyName("PluginA.dll")
            // uncomment line below to add the support to load multiple plugins from the assembly
            //.SupportMultiplePlugins()
            );
        }

        private IServiceCollection AddPriseWithCustomerLoader(IServiceCollection services)
        {
            // This will look for a custom plugin based on the context
            return services.AddPriseWithCustomLoader<ICalculationPlugin, ContextPluginLoader<ICalculationPlugin>>(options =>
                 options
                     .WithDefaultOptions($"{Env.ContentRootPath}\\PluginServer")
             );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            var provider = new FileExtensionContentTypeProvider();
            // allow DLL downloads
            provider.Mappings[".dll"] = "application/x-msdownload";
            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "Plugins")),
                RequestPath = "/Plugins",
                ContentTypeProvider = provider
            });

            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "Plugins")),
                RequestPath = "/Plugins"
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
