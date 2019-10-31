using System;
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

using Contract;
using Prise.Infrastructure.NetCore;
using System.IO;
using MyHost.Infrastructure;

namespace MyHost
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            var tenantConfig = new TenantConfig();
            Configuration.Bind("TenantConfig", tenantConfig);
            services.AddSingleton(tenantConfig); // Add the tenantConfig for use in the Tenant Aware middleware

            services.AddHttpContextAccessor(); // Add the IHttpContextAccessor for use in the Tenant Aware middleware

            services.AddPrise<IProductsRepository>(options => options
                // Plugins will be located at /bin/Debug/netcoreapp3.0/Plugins directory
                // each plugin will have its own directory
                // /SQLPlugin
                // /TableStoragePlugin
                // /CosmosDbPlugin
                .WithDefaultOptions(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"))
                .WithPluginAssemblyNameProvider<TenantPluginAssemblyNameProvider>()
                .WithLocalDiskAssemblyLoader<TenantPluginAssemblyLoadOptions>()
                .ConfigureSharedServices(services =>
                {
                    // Add the configuration for use in the plugins
                    // this way, the plugins can read their own config section from the appsettings.json
                    services.AddSingleton(Configuration); 
                })
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
