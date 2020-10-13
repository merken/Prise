using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
#if !NETCORE2_1
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
#endif
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Prise.DependencyInjection;
using Prise.Core;
using Prise.Activation;
using Prise.AssemblyLoading;
using Prise.AssemblyScanning;
using Prise.Proxy;
using Prise.Caching;

namespace Prise.Mvc
{
    public static class PriseMvcExtensions
    {
        internal static PluginLoadContext AddMvcTypes(this PluginLoadContext loadContext)
        {
            return loadContext.AddHostTypes(new[] { typeof(ControllerBase) });
        }

        internal static PluginLoadContext AddMvcRazorTypes(this PluginLoadContext loadContext)
        {
            return loadContext.AddHostTypes(new[] { typeof(ControllerBase), typeof(ITempDataDictionaryFactory) });
        }

        public static IServiceCollection AddCorePriseServices(this IServiceCollection services)
        {
            return services
                    .AddFactory<IAssemblyScanner>(DefaultFactories.DefaultAssemblyScanner, ServiceLifetime.Scoped)
                    .AddFactory<IPluginTypeSelector>(DefaultFactories.DefaultPluginTypeSelector, ServiceLifetime.Scoped)
                    .AddFactory<IParameterConverter>(DefaultFactories.DefaultParameterConverter, ServiceLifetime.Scoped)
                    .AddFactory<IResultConverter>(DefaultFactories.DefaultResultConverter, ServiceLifetime.Scoped)
                    .AddFactory<IPluginActivationContextProvider>(DefaultFactories.DefaultPluginActivationContextProvider, ServiceLifetime.Scoped)
                    .AddFactory<IRemotePluginActivator>(DefaultFactories.DefaultRemotePluginActivator, ServiceLifetime.Scoped)
                    .AddFactory<IPluginProxyCreator>(DefaultFactories.DefaultPluginProxyCreator, ServiceLifetime.Scoped)
                    .AddFactory<IAssemblyLoader>(DefaultFactories.DefaultAssemblyLoader, ServiceLifetime.Singleton);
        }

        /// <summary>
        /// Does all of the plumbing to load API Controllers as Prise Plugins.
        /// Limitiations:
        /// - No DispatchProxy can be used, backwards compatability is compromised (DispatchProxy requires an interface as base class, not ControllerBase)
        /// - Plugin Cache is set to Singleton because we cannot unload assemblies, this would disable the controller routing (and result in 404)
        /// - Assembly unloading is disabled, memory leaks can occur
        /// </summary>
        /// <returns>A fully configured Prise setup that allows you to load plugins via the IMvcPluginLoader</returns>
        public static IServiceCollection AddPriseMvc(this IServiceCollection services)
        {

            return services
                    .AddCorePriseServices()
                    .AddSingleton<IPluginCache, DefaultScopedPluginCache>()
                    .AddScoped<IMvcPluginLoader, DefaultMvcPluginLoader>()
                    .ConfigureMvcServices()
                ;
        }

        /// <summary>
        /// Does all of the plumbing to load API Controllers and RAZOR Controllers as Prise Plugins.
        /// Limitiations:
        /// - No DispatchProxy can be used, backwards compatability is compromised (DispatchProxy requires an interface as base class, not ControllerBase)
        /// - Plugin Cache is set to Singleton because we cannot unload assemblies, this would disable the controller routing (and result in 404)
        /// - Assembly unloading is disabled, memory leaks can occur
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="webRootPath">By default, this should be the IWebHostEnvironment.WebRootPaht or IHostingEnvironment.WebRootPath</param>
        /// <returns></returns>
        public static IServiceCollection AddPriseRazorPlugins(this IServiceCollection services, string webRootPath, string pathToPlugins)
        {
            return services
                    .AddCorePriseServices()
                    .AddSingleton<IPluginCache, DefaultScopedPluginCache>()
                    .AddScoped<IMvcPluginLoader, DefaultMvcRazorPluginLoader>()
                    .ConfigureMvcServices()
                    .ConfigureRazorServices(webRootPath, pathToPlugins)
                ;
        }
        //             ;

        //             return builder
        //                 // Use a singleton cache
        //                 .WithSingletonCache()
        //                 .ConfigureServices(services =>
        //                     services
        //                         .ConfigureMVCServices<T>()
        //                         .ConfigureRazorServices<T>(webRootPath))
        // #if NETCORE2_1
        //                  // This is required in 2.1 because there is no AssemblyDependencyResolver
        //                  .UsePluginContextAsDependencyPath()
        // #endif
        //                  // Makes sure controllers can be casted to the host's representation of ControllerBase
        //                  .WithHostType(typeof(ControllerBase))
        //                  // Makes sure the Microsoft.AspNetCore.Mvc.ViewFeatures assembly is loaded from the host
        //                  .WithHostType(typeof(ITempDataDictionaryFactory))
        //             ;
        //         }

        private static IServiceCollection ConfigureMvcServices(this IServiceCollection services)
        {
            var actionDescriptorChangeProvider = new DefaultPriseMvcActionDescriptorChangeProvider();
            // Registers the change provider
            return services
                .AddSingleton<IPriseMvcActionDescriptorChangeProvider>(actionDescriptorChangeProvider)
                .AddSingleton<IActionDescriptorChangeProvider>(actionDescriptorChangeProvider)
                // Registers the activator for controllers from plugin assemblies
                .Replace(ServiceDescriptor.Transient<IControllerActivator, DefaultPriseMvcControllerActivator>());
        }

        private static IServiceCollection ConfigureRazorServices(this IServiceCollection services, string webRootPath, string pathToPlugins)
        {
            return services
#if NETCORE2_1
                .Configure<RazorViewEngineOptions>(options =>
                {
                    options.FileProviders.Add(new PrisePluginViewsAssemblyFileProvider(webRootPath, pathToPlugins));
                })
#else
                .Configure<MvcRazorRuntimeCompilationOptions>(options =>
                {
                    options.FileProviders.Add(new DefaultPrisePluginViewsAssemblyFileProvider(webRootPath, pathToPlugins));
                })
#endif
                // Registers the static Plugin Cache Accessor
                .AddSingleton<IPluginCacheAccessorBootstrapper, DefaultStaticPluginCacheAccessorBootstrapper>();
        }

    }
}
