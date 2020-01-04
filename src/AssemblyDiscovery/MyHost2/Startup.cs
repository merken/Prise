using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Contract;
using System.IO;
using Prise;
using System.Linq;
using Prise.AssemblyScanning.Discovery;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

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

            services.AddHttpClient();
            services.AddHttpContextAccessor(); // Add the IHttpContextAccessor for use in the Tenant Aware middleware

            //AddPriseWithoutAssemblyScanning(services);
            AddPriseWithAssemblyScanning<IProductsReader>(services);
            AddPriseWithAssemblyScanning<IProductsWriter>(services);
            AddPriseWithAssemblyScanning<IProductsDeleter>(services);
        }

        private void AddPriseWithAssemblyScanning<T>(IServiceCollection services)
            where T : class
        {
            services.AddPrise<T>(options => options
                .WithDefaultOptions(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"))
                .ScanForAssemblies(composer =>
                    composer.UseDiscovery())
                .ConfigureSharedServices(sharedServices =>
                {
                    sharedServices.AddSingleton(Configuration);
                })
            );
        }

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
