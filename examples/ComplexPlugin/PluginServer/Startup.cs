using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;

using Prise.Infrastructure.NetCore;
using PluginContract;
using PluginServer.Services;
using PluginServer.Custom;
using System;

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

            var cla = services.BuildServiceProvider().GetRequiredService<ICommandLineArguments>();

            if (cla.UseLazyService)
            {
                services.AddScoped<ICalculationService, LazyCalculationService>();
            }
            else
            {
                services.AddScoped<ICalculationService, EagerCalculationService>();
            }

            AddPriseWithContextBasedPluginLoading(services);
        }

        private IServiceCollection AddPriseWithContextBasedPluginLoading(IServiceCollection services)
        {
            // This will look for a custom plugin based on the context
            return services.AddPrise<ICalculationPlugin>(options =>
                 options
                     .WithDefaultOptions(Path.Combine(GetExecutionDirectory(), "Plugins"))
                     .WithPluginAssemblyNameProvider<ContextPluginAssemblyNameProvider>()
                     .WithLocalDiskAssemblyLoader<ContextPluginAssemblyLoadOptions>()
             );
        }

        private string GetExecutionDirectory() => AppDomain.CurrentDomain.BaseDirectory;

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
