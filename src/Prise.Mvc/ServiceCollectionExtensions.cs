using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
#if NETCORE3_0 || NETCORE3_1
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
#endif
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Prise.Mvc.Infrastructure;

namespace Prise.Mvc
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Does all of the plumbing to load API Controllers as Prise Plugins.
        /// Limitiations:
        /// - No DispatchProxy can be used, backwards compatability is compromised (DispatchProxy requires an interface as base class, not ControllerBase)
        /// - Plugin Cache is set to Singleton because we cannot unload assemblies, this would disable the controller routing (and result in 404)
        /// - Assembly unloading is disabled, memory leaks can occur
        /// </summary>
        /// <typeparam name="T">The Plugin Contract Type</typeparam>
        /// <param name="builder"></param>
        /// <returns>A fully configured Prise setup that will load Controllers from Plugin Assemblies</returns>
        public static IServiceCollection AddPriseControllersAsPlugins<T>(this IServiceCollection services)
        {

            return services
                .AddSingleton(typeof(DefaultScopedPluginCache<T>))
                .ConfigureMVCServices<T>()

                ;
            //             return builder
            //                  // Use a singleton cache
            //                  .WithSingletonCache()
            //                  .ConfigureServices(services =>
            //                     services
            //                         .ConfigureMVCServices<T>())
            // #if NETCORE2_1
            //                  // This is required in 2.1 because there is no AssemblyDependencyResolver
            //                  .UsePluginContextAsDependencyPath()
            // #endif
            //                  // Makes sure controllers can be casted to the host's representation of ControllerBase
            //                  .WithHostType(typeof(ControllerBase))
            //             ;
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
        public static PluginLoadOptionsBuilder<T> AddPriseRazorPlugins<T>(this IServiceCollection services, string webRootPath)
        {
            return services
                    .AddSingleton(typeof(DefaultScopedPluginCache<T>))
                    .ConfigureMVCServices<T>()
                    .ConfigureRazorServices<T>(webRootPath)

            ;

            return builder
                // Use a singleton cache
                .WithSingletonCache()
                .ConfigureServices(services =>
                    services
                        .ConfigureMVCServices<T>()
                        .ConfigureRazorServices<T>(webRootPath))
#if NETCORE2_1
                 // This is required in 2.1 because there is no AssemblyDependencyResolver
                 .UsePluginContextAsDependencyPath()
#endif
                 // Makes sure controllers can be casted to the host's representation of ControllerBase
                 .WithHostType(typeof(ControllerBase))
                 // Makes sure the Microsoft.AspNetCore.Mvc.ViewFeatures assembly is loaded from the host
                 .WithHostType(typeof(ITempDataDictionaryFactory))
            ;
        }

        private static IServiceCollection ConfigureMVCServices<T>(this IServiceCollection services)
        {
            var actionDescriptorChangeProvider = new PriseActionDescriptorChangeProvider();
            // Registers the change provider
            return services
                .AddSingleton<IPriseActionDescriptorChangeProvider>(actionDescriptorChangeProvider)
                .AddSingleton<IActionDescriptorChangeProvider>(actionDescriptorChangeProvider)
                // Registers the activator for controllers from plugin assemblies
                .Replace(ServiceDescriptor.Transient<IControllerActivator, PriseControllersAsPluginActivator<T>>());
        }

        private static IServiceCollection ConfigureRazorServices<T>(this IServiceCollection services, string webRootPath)
        {
            return services
#if NETCORE2_1
                .Configure<RazorViewEngineOptions>(options =>
                {
                    options.FileProviders.Add(new PrisePluginViewsAssemblyFileProvider<T>(webRootPath));
                })
#endif
#if NETCORE3_0 || NETCORE3_1
                .Configure<MvcRazorRuntimeCompilationOptions>(options =>
                {
                    options.FileProviders.Add(new PrisePluginViewsAssemblyFileProvider<T>(webRootPath));
                })
#endif
                // Registers the static Plugin Cache Accessor
                .AddSingleton<IPluginCacheAccessorBootstrapper<T>, StaticPluginCacheAccessorBootstrapper<T>>();
        }

    }
}
