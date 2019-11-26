using Contract;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prise;
using System;
using System.IO;

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
            services.AddHttpContextAccessor();
            services.AddScoped<IExternalService, AcceptHeaderlanguageService>();

            services.AddPrise<IHelloPlugin>(options => options
                .WithDefaultOptions(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "LanguageBased.Plugin"))
                .WithPluginAssemblyName("LanguageBased.Plugin.dll")
                .IgnorePlatformInconsistencies() // The plugin is a netstandard library, the host is a netcoreapp, ignore this inconsistency
                .ConfigureSharedServices(sharedServices =>
                {
                    var myHostServiceProvider = services.BuildServiceProvider();
                    var acceptheaderLanguageService = myHostServiceProvider.GetRequiredService<IExternalService>();
                    // Gets the AcceptHeaderlanguageService and adds it to the ServiceCollection for creating the LanguageBasedPlugin
                    // This can be added as a Singleton, since the scope is defined by the MyHost, and this is always Scoped.
                    sharedServices.AddSingleton<IExternalService>(acceptheaderLanguageService);
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
