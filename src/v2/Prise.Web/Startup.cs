using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prise.Console.Contract;
using Prise.DependencyInjection;

namespace Prise.Web
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

            var pathToThisProgram = Assembly.GetExecutingAssembly() // this assembly location (/bin/Debug/netcoreapp3.1)
                                       .Location;
            var pathToExecutingDir = Path.GetDirectoryName(pathToThisProgram);
            var pathToSinglePlugin = Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../Packages/dist/Prise.Plugin.Single"));
            var pathToMultiplePlugins = Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../Packages/dist"));
            
            // services.AddPrise<IPlugin>(pathToSinglePlugin, true);

            // services.AddPrise<IMultiplePlugin>(
            //     pathToMultiplePlugins,
            //     allowMultiple:true,
            //     ignorePlatormInconsistencies: true);

            // services.AddPrise<IStoragePlugin>(pathToMultiplePlugins,
            //     allowMultiple:true,
            //     ignorePlatormInconsistencies: true,
            //     includeHostServices: new[] { typeof(IConfiguration) },
            //     sharedServices: (sharedServices) =>
            //     {
            //         sharedServices.AddScoped<IConfigurationService, AppSettingsConfigurationService>();
            //     });
            services.AddPrise();
            services.AddScoped<IPluginLoader, PluginLoader>();
            services.AddScoped<IConfigurationService, AppSettingsConfigurationService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
