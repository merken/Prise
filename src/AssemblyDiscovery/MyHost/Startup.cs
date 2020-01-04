using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Contract;
using System.IO;
using Prise;
using System.Linq;
using Prise.AssemblyScanning.Discovery;
using System.Threading.Tasks;

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

            services.AddHttpClient();
            services.AddHttpContextAccessor(); // Add the IHttpContextAccessor for use in the Tenant Aware middleware

            //AddPriseWithoutAssemblyScanning(services);
            AddPriseWithAssemblyScanning<IProductsReader>(services);
            AddPriseWithAssemblyScanning<IProductsWriter>(services);
            AddPriseWithAssemblyScanning<IProductsDeleter>(services);
        }

        private void AddPriseWithoutAssemblyScanning(IServiceCollection services)
        {
            services.AddPrise<IProductsReader>(options => options
                .WithDefaultOptions(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("Plugins", "ProductsReaderPlugin")))
                .WithPluginAssemblyName("ProductsReaderPlugin.dll")
                .ConfigureSharedServices(services =>
                {
                    services.AddSingleton(Configuration);
                })
            );

            services.AddPrise<IProductsWriter>(options => options
                .WithDefaultOptions(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("Plugins", "ProductsWriterPlugin")))
                .WithPluginAssemblyName("ProductsWriterPlugin.dll")
                .ConfigureSharedServices(services =>
                {
                    services.AddSingleton(Configuration);
                })
            );

            services.AddPrise<IProductsDeleter>(options => options
                .WithDefaultOptions(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("Plugins", "ProductsDeleterPlugin")))
                .WithPluginAssemblyName("ProductsDeleterPlugin.dll")
                .ConfigureSharedServices(services =>
                {
                    services.AddSingleton(Configuration);
                })
            );
        }

        private void AddPriseWithAssemblyScanning<T>(IServiceCollection services)
            where T : class
        {
            services.AddPrise<T>(options => options
                .WithDefaultOptions(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"))
                .ScanForAssemblies(composer =>
                    composer.UseDiscovery())
                .ConfigureSharedServices(services =>
                {
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
