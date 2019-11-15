using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Contract;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyHost.Infrastructure;
using Prise.Infrastructure;
using Prise.Infrastructure.NetCore;

namespace MyHost2
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var tenantConfig = new TenantConfig();
            Configuration.Bind("TenantConfig", tenantConfig);
            services.AddSingleton(tenantConfig); // Add the tenantConfig for use in the Tenant Aware middleware

            services.AddHttpClient();
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
                //.WithNetworkAssemblyLoader<TenantPluginNetworkLoadOptions>()

                .WithDependencyPathProvider<TenantPluginDependencyPathProvider>()
                .ConfigureSharedServices(sharedServices =>
                {
                    // Add the configuration for use in the plugins
                    // this way, the plugins can read their own config section from the appsettings.json
                    sharedServices.AddSingleton(Configuration);
                })
                .WithHostType<BinderOptions>()
                //.WithSelector((types) =>
                //{
                //    return types.Where(t => t.Name.Contains("MongoDbProductsRepository"));
                //})
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
