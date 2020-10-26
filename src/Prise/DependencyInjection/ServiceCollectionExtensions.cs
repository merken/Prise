using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Prise.Activation;
using Prise.AssemblyLoading;
using Prise.AssemblyScanning;
using Prise.Proxy;

namespace Prise.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// This bootstrapper adds the basic Prise services in order to do manual Plugin loading.
        /// </summary>
        /// <param name="serviceLifetime">The path to the directory to scan for plugins</param>
        /// <returns>A ServiceCollection that has the following types registered and ready for injection: 
        /// <see cref="IAssemblyScanner"/>, <see cref="IPluginTypeSelector"/>, <see cref="IParameterConverter"/> ,<see cref="IResultConverter"/>, <see cref="IPluginActivator"/>, <see cref="IAssemblyLoader"/> and <see cref="IPluginLoader"/>
        /// </returns>
        public static IServiceCollection AddPrise(this IServiceCollection services,
                                                     ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            return services
                    // Add all the factories
                    .AddFactory<IAssemblyScanner>(DefaultFactories.DefaultAssemblyScanner, serviceLifetime)
                    .AddFactory<IPluginTypeSelector>(DefaultFactories.DefaultPluginTypeSelector, serviceLifetime)
                    .AddFactory<IParameterConverter>(DefaultFactories.DefaultParameterConverter, serviceLifetime)
                    .AddFactory<IResultConverter>(DefaultFactories.DefaultResultConverter, serviceLifetime)
                    .AddFactory<IPluginActivator>(DefaultFactories.DefaultPluginActivator, serviceLifetime)
                    .AddFactory<IAssemblyLoader>(DefaultFactories.DefaultAssemblyLoader, serviceLifetime)
                    // Add the loader
                    .AddService(new ServiceDescriptor(typeof(IPluginLoader), typeof(DefaultPluginLoader), serviceLifetime))
                ;
        }

        /// <summary>
        /// This is the bootstrapper method to register a Plugin of <see cref="{T}"> using Prise
        /// </summary>
        /// <param name="pathToPlugin">The path to start scanning for plugins</param>
        /// <param name="hostFramework">The framework of the host, optional</param>
        /// <param name="allowMultiple">If <true>, an <see cref="IEnumerable<{T}>"/> is registered, all plugins of this type will have the same configuration. If <false> only the first found Plugin is registered</param>
        /// <param name="configureContext">A builder function that you can use configure the load context</param>
        /// <typeparam name="T">The Plugin type</typeparam>
        /// <returns>A full configured ServiceCollection that will resolve <see cref="{T}"/> or an <see cref="IEnumerable<{T}>"/> based on <allowMultiple></returns>
        public static IServiceCollection AddPrise<T>(this IServiceCollection services,
                                                            Func<IServiceProvider, string> pathToPlugin,
                                                            Func<IServiceProvider, string> hostFramework = null,
                                                            bool allowMultiple = false,
                                                            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped,
                                                            Action<PluginLoadContext> configureContext = null)
                   where T : class
        {
            return services
                        .AddPrise(serviceLifetime) // Adds the default Prise Services
                        .AddService(new ServiceDescriptor(allowMultiple ? typeof(IEnumerable<T>) : typeof(T), (sp) =>
                        {
                            var loader = sp.GetRequiredService<IPluginLoader>();
                            var scanResults = loader.FindPlugins<T>(pathToPlugin.Invoke(sp)).Result;
                            if (!scanResults.Any())
                                throw new PriseDependencyInjectionException($"No plugin assembly was found for plugin type {typeof(T).Name}");

                            if (!allowMultiple) // Only 1 plugin is requested
                                return loader.LoadPlugin<T>(scanResults.First(), hostFramework?.Invoke(sp), configureContext).Result;

                            var plugins = new List<T>();
                            foreach (var scanResult in scanResults)
                                plugins.AddRange(loader.LoadPlugins<T>(scanResult, hostFramework?.Invoke(sp), configureContext).Result);

                            return plugins;
                        }, serviceLifetime))
           ;
        }

        public static IServiceCollection AddFactory<T>(this IServiceCollection services, Func<T> func, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
                    where T : class
        {
            Func<IServiceProvider, Func<T>> factoryOfFuncT = (sp) => func;
            Func<IServiceProvider, T> factoryOfT = (sp) => sp.GetRequiredService<Func<T>>()() as T;
            return services
                .AddService(new ServiceDescriptor(typeof(Func<T>), factoryOfFuncT, serviceLifetime))
                .AddService(new ServiceDescriptor(typeof(T), factoryOfT, serviceLifetime));
        }

        private static IServiceCollection AddService(this IServiceCollection services, ServiceDescriptor serviceDescriptor)
        {
            services
               .Add(serviceDescriptor);
            return services;
        }
    }
}