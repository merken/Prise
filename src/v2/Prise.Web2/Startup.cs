using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prise.Console.Contract;
using Prise.DependencyInjection;
using Prise.Web;

namespace Prise.Web2
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

            var pathToThisProgram = Assembly.GetExecutingAssembly() // this assembly location (/bin/Debug/netcoreapp3.1)
                                       .Location;
            var pathToExecutingDir = Path.GetDirectoryName(pathToThisProgram);
            var pathToSinglePlugin = Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../Packages/dist/Prise.Plugin.Single"));
            var pathToMultiplePlugins = Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../Packages/dist"));
            services.AddPrise<IPlugin>(pathToSinglePlugin, true);
            services.AddPrisePlugins<IMultiplePlugin>(pathToMultiplePlugins,
                                                      ignorePlatormInconsistencies: true);

            services.AddPrisePlugins<IStoragePlugin>(pathToMultiplePlugins,
                                                     ignorePlatormInconsistencies: true,
                                                     includeHostServices: new[] { typeof(IConfiguration) },
                                                     sharedServices: (sharedServices) =>
                                                     {
                                                         sharedServices.AddScoped<IConfigurationService, AppSettingsConfigurationService>();
                                                     });
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
            app.UseMvc();
        }
    }
}
