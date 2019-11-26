using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;

using Prise;
using Contract;
using AppHost.Services;
using AppHost.Custom;
using System;

namespace AppHost
{
    public partial class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureTargetFramework(services);
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
                     .WithDefaultOptions()
                     .IgnorePlatformInconsistencies()
                     .WithPluginPathProvider<ContextPluginPathProvider<ICalculationPlugin>>()
                     .WithPluginAssemblyNameProvider<ContextPluginAssemblyNameProvider<ICalculationPlugin>>()
                     .WithHostFrameworkProvider<AppHostFrameworkProvider>()
             );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
#if NETCORE3_0
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
#endif
#if NETCORE2_1
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
#endif
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            ConfigureTargetFramework(app);
        }
    }
}
