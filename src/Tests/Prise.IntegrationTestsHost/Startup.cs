using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;

using Prise;
using Prise.IntegrationTestsContract;
using Prise.IntegrationTestsHost.Services;
using Prise.IntegrationTestsHost.Custom;
using System;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Prise.AssemblyScanning.Discovery;
using ExternalServices;
using Prise.IntegrationTestsHost.Controllers;

namespace Prise.IntegrationTestsHost
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
            services.AddHttpClient(); // Required for the INetworkCalculationPlugin
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

            AddPriseCalculationPlugins(services);
            AddPriseNetworkCalculationPlugins(services);
            AddPriseTranslationPlugins(services);
            AddPriseDataServicesPlugin(services);
            AddPriseSerializerPlugin(services, cla.UseCollectibleAssemblies);
        }

        protected virtual IServiceCollection AddPriseCalculationPlugins(IServiceCollection services)
        {
            // This will look for a custom plugin based on the context
            return services
                // Registers the default ICalculationPlugin
                .AddPrise<ICalculationPlugin>(options =>
                     options
                        .WithDefaultOptions()
                        .IgnorePlatformInconsistencies()
                        .WithPluginPathProvider<ContextPluginPathProvider<ICalculationPlugin>>()
                        .WithPluginAssemblyNameProvider<ContextPluginAssemblyNameProvider<ICalculationPlugin>>()
                        .WithHostFrameworkProvider<AppHostFrameworkProvider>()
                 );
        }

        protected virtual IServiceCollection AddPriseNetworkCalculationPlugins(IServiceCollection services)
        {
            return services
                // Registers the plugin that will be loaded over the network
                .AddPrise<INetworkCalculationPlugin>(options =>
                     options
                        .WithDefaultOptions()
                        .WithNetworkAssemblyLoader<NetworkPluginProvider>()
                        .WithPluginAssemblyNameProvider<NetworkPluginProvider>()
                        .WithHostFrameworkProvider<AppHostFrameworkProvider>()
                        .ConfigureSharedServices(
                            sharedServices =>
                                sharedServices
                                .AddSingleton(Configuration)
                            )
                 );
        }
        
        protected virtual IServiceCollection AddPriseTranslationPlugins(IServiceCollection services)
        {
            return services
                // Registers the plugin that will be loaded via scanning
                .AddPrise<ITranslationPlugin>(options =>
                     options
                        .WithDefaultOptions(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"))
                        .ScanForAssemblies(composer =>
                            composer.UseDiscovery())
                        .WithHostFrameworkProvider<AppHostFrameworkProvider>()
                        .ConfigureHostServices(hostServices =>
                        {
                            // These services are registered as host types
                            // Their types and instances will be loaded from the MyHost
                            hostServices.AddHttpContextAccessor();
                            hostServices.AddSingleton<IConfiguration>(Configuration);
                        })
                        .ConfigureSharedServices(sharedServices =>
                        {
                            // The AcceptHeaderlanguageService is known in the Host, but the type is registered as a remote type
                            // This encourages backwards compatability
                            sharedServices.AddTransient<ICurrentLanguageProvider, AcceptHeaderLanguageProvider>();
                        })
                 );
        }

        protected virtual IServiceCollection AddPriseDataServicesPlugin(IServiceCollection services)
        {
            return services
                .AddPrise<IAuthenticatedDataService>(options =>
                     options
                        .WithDefaultOptions(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "PluginE"))
                        .WithPluginAssemblyName("PluginE.dll")
                        .WithHostFrameworkProvider<AppHostFrameworkProvider>()
                 )

                .AddPrise<ITokenService>(options =>
                     options
                        .WithDefaultOptions(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "PluginE"))
                        .WithPluginAssemblyName("PluginE.dll")
                        .WithHostFrameworkProvider<AppHostFrameworkProvider>()
                 );
        }

        protected virtual IServiceCollection AddPriseSerializerPlugin(IServiceCollection services, bool isCollectable)
        {
            return services
                .AddPrise<IPluginWithSerializer>(options =>
                         options
                            .WithDefaultOptions(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "PluginF"))
                            .WithPluginAssemblyName("PluginF.dll")
                            .WithSelector<SerializationPluginSelector>()
                            .UseCollectibleAssemblies(isCollectable)
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

            var provider = new FileExtensionContentTypeProvider();
            // Add new mappings
            provider.Mappings[".dll"] = "application/x-msdownload";
            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "Plugins")),
                RequestPath = "/Plugins",
                ContentTypeProvider = provider
            });

            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "Plugins")),
                RequestPath = "/Plugins"
            });
        }
    }
}
