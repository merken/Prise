using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Prise.Activation;
using Prise.AssemblyLoading;
using Prise.AssemblyScanning;
using Prise.Core;
using Prise.Proxy;
using Prise.Utils;

namespace Prise.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {

        /// <summary>
        /// This bootstrapper adds the basic Prise services in order to do manual Plugin loading.
        /// </summary>
        /// <param name="serviceLifetime">The path to the directory to scan for plugins</param>
        /// <returns>A ServiceCollection that has the following types registered and ready for injection: 
        /// <IAssemblyScanner>, <IPluginTypeSelector>, <IParameterConverter> ,<IResultConverter>, <IPluginActivator> and <IAssemblyLoader>
        /// Get started writing your own Plugin Loader: [https://github.com/merken/Prise/blob/master/Writing-Your-Own-Loader.md]
        /// </returns>
        public static IServiceCollection AddPrise(this IServiceCollection services,
                                                     ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            return services
                    .AddFactory<IAssemblyScanner>(DefaultFactories.DefaultAssemblyScanner, serviceLifetime)
                    .AddFactory<IPluginTypeSelector>(DefaultFactories.DefaultPluginTypeSelector, serviceLifetime)
                    .AddFactory<IParameterConverter>(DefaultFactories.DefaultParameterConverter, serviceLifetime)
                    .AddFactory<IResultConverter>(DefaultFactories.DefaultResultConverter, serviceLifetime)
                    .AddFactory<IPluginActivator>(DefaultFactories.DefaultPluginActivator, serviceLifetime)
                    .AddFactory<IAssemblyLoader>(DefaultFactories.DefaultAssemblyLoader, serviceLifetime);
        }

        /// <summary>
        /// This is the bootstrapper method to register a Plugin of <T> using Prise
        /// </summary>
        /// <param name="pathToPlugins">The path to start scanning for plugins</param>
        /// <param name="allowMultiple">If <true>, an IEnumerable<T> is registered, all plugins of this type will have the same configuration. If <false> only the first found Plugin is registered</param>
        /// <param name="ignorePlatormInconsistencies">Set to <true> if you wish to load netstandard plugins</param>
        /// <param name="hostFramework">The framework of the host, optional</param>
        /// <param name="includeHostServices">A list of service types from the Host you wish to share with the Plugin</param>
        /// <param name="excludeHostServices">A list of service types from the Host you wish to exlude from sharing with the Plugin</param>
        /// <param name="hostServices">A builder function that you can use to add Host services to share with the Plugin, accumulates with includeHostServices</param>
        /// <param name="sharedServices">A builder function that you can use to add Shared services to the Plugin</param>
        /// <param name="hostAssemblies">A list of Assembly names that should always be loaded from the Host</param>
        /// <param name="remoteTypes">A list of Types that should always be loaded from the Plugin</param>
        /// <param name="downgradableHostTypes">A list of Types that are allowed to be downgraded, this happens when the Plugin is loading a newer version of a Host assembly. Does not always guarantee success.</param>
        /// <param name="downgradableHostAssemblies">A list of Assemblies that are allowed to be downgraded, this happens when the Plugin is loading a newer version of a Host assembly. Does not always guarantee success.</param>
        /// <param name="additionalProbingPaths">Additional absolute paths you wish to load Plugin dependencies from</param>
        /// <typeparam name="T">The Plugin type</typeparam>
        /// <returns>A full configured ServiceCollection that will resolve <T> or an IEnumerable<T> based on <allowMultiple></returns>
        public static IServiceCollection AddPrise<T>(this IServiceCollection services,
                                                            string pathToPlugins,
                                                            bool allowMultiple = false,
                                                            bool ignorePlatormInconsistencies = false,
                                                            string hostFramework = null,
                                                            IEnumerable<Type> includeHostServices = null,
                                                            IEnumerable<Type> excludeHostServices = null,
                                                            Action<IServiceCollection> hostServices = null,
                                                            Action<IServiceCollection> sharedServices = null,
                                                            IEnumerable<string> hostAssemblies = null,
                                                            IEnumerable<Type> remoteTypes = null,
                                                            IEnumerable<Type> downgradableHostTypes = null,
                                                            IEnumerable<string> downgradableHostAssemblies = null,
                                                            IEnumerable<string> additionalProbingPaths = null)
                   where T : class
        {
            var serviceLifetime = ServiceLifetime.Scoped;

            return services
                        .AddPrise() // Adds the default Prise Services
                        .AddService(new ServiceDescriptor(allowMultiple ? typeof(IEnumerable<T>) : typeof(T), (sp) =>
                        {
                            var frameworkFromHost = hostFramework ?? HostFrameworkUtils.GetHostframeworkFromHost();
                            var scanner = sp.GetRequiredService<IAssemblyScanner>();
                            var loader = sp.GetRequiredService<IAssemblyLoader>();
                            var selector = sp.GetRequiredService<IPluginTypeSelector>();
                            var activator = sp.GetRequiredService<IPluginActivator>();
                            var parameterConverter = sp.GetRequiredService<IParameterConverter>();
                            var resultConverter = sp.GetRequiredService<IResultConverter>();

                            var scanResults = scanner.Scan(new AssemblyScannerOptions
                            {
                                StartingPath = pathToPlugins,
                                PluginType = typeof(T)
                            }).Result;

                            if (!scanResults.Any())
                                throw new PriseDependencyInjectionException($"No plugin assembly was found for plugin type {typeof(T).Name}");

                            var plugins = new List<T>();
                            foreach (var scanResult in scanResults)
                            {
                                var pluginLoadContext = PluginLoadContext.DefaultPluginLoadContext(Path.Combine(scanResult.AssemblyPath, scanResult.AssemblyName), typeof(T), frameworkFromHost);
                                pluginLoadContext.IgnorePlatformInconsistencies = ignorePlatormInconsistencies;

                                IServiceCollection hostServicesCollection = new ServiceCollection();
                                
                                hostServices?.Invoke(hostServicesCollection);

                                pluginLoadContext
                                    .AddHostServices(services, hostServicesCollection, includeHostServices, excludeHostServices ?? Enumerable.Empty<Type>())
                                    .AddHostAssemblies(hostAssemblies)
                                    .AddRemoteTypes(remoteTypes)
                                    .AddDowngradableHostTypes(downgradableHostTypes)
                                    .AddDowngradableHostAssemblies(downgradableHostAssemblies)
                                    .AddAdditionalProbingPaths(additionalProbingPaths)
                                ;

                                var sharedServicesCollection = new ServiceCollection();
                                sharedServices?.Invoke(sharedServicesCollection);
                                
                                foreach (var sharedService in sharedServicesCollection)
                                    pluginLoadContext.AddSharedService(sharedService);

                                var pluginAssembly = loader.Load(pluginLoadContext).Result;
                                var pluginTypes = selector.SelectPluginTypes<T>(pluginAssembly);

                                if (!allowMultiple)
                                    pluginTypes = pluginTypes.Take(1);

                                foreach (var pluginType in pluginTypes)
                                {
                                    plugins.Add(activator.ActivatePlugin<T>(new Activation.DefaultPluginActivationOptions
                                    {
                                        PluginType = pluginType,
                                        PluginAssembly = pluginAssembly,
                                        ParameterConverter = parameterConverter,
                                        ResultConverter = resultConverter,
                                        HostServices = hostServicesCollection,
                                        SharedServices = sharedServicesCollection
                                    }).Result);
                                }
                            }
                            return plugins;
                        }, serviceLifetime))

           ;
        }

        private static IServiceCollection AddFactory<T>(this IServiceCollection services, Func<T> func, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
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