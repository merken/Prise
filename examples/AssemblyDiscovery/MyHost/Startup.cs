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
    public class FakeWriter : IProductsWriter
    {
        public Task<Product> Create(Product product)
        {
            throw new NotImplementedException();
        }

        public Task<Product> Update(Product product)
        {
            throw new NotImplementedException();
        }
    }

    public class FakeDeleter : IProductsDeleter
    {
        public Task Delete(int productId)
        {
            throw new NotImplementedException();
        }
    }
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

            services.AddScoped<IProductsWriter, FakeWriter>(); // TODO
            services.AddScoped<IProductsDeleter, FakeDeleter>(); // TODO

            services.AddPrise<IProductsReader>(options => options
                .WithDefaultOptions(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"))
                .ScanForAssemblies(composer =>
                    composer.UseDiscovery())
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
