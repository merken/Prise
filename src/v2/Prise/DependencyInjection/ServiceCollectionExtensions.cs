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
using Prise.Infrastructure;

namespace Prise.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Using this bootstrapper method, you can register your Plugin Contract <T> so that <T> can be injected into any service (service, api controller, razor controller, ... )
        /// </summary>
        /// <param name="pathToPlugins">The path to the directory to scan for plugins</param>
        /// <param name="ignorePlatormInconsistencies">Set to true if the target framework of the host (netcoreapp3.1) will potentially differ from the plugin(netcoreapp2.1, netstandard2.0)</param>
        /// <typeparam name="T">The Plugin Contract</typeparam>
        public static IServiceCollection AddPrise<T>(this IServiceCollection services,
                                                     string pathToPlugins,
                                                     bool ignorePlatormInconsistencies = false,
                                                     string hostFramework = null,
                                                     IEnumerable<Type> includeHostServices = null,
                                                     IEnumerable<Type> excludeHostServices = null,
                                                     Action<IServiceCollection> hostServices = null,
                                                     Action<IServiceCollection> sharedServices = null,
                                                     IEnumerable<string> hostAssemblies = null,
                                                     IEnumerable<Type> remoteTypes = null,
                                                     IEnumerable<Type> downgradableTypes = null,
                                                     IEnumerable<string> downgradableHostAssemblies = null,
                                                     IEnumerable<string> additionalProbingPaths = null)
            where T : class
        {
            var serviceLifetime = ServiceLifetime.Scoped;
            return services
                        .AddService(new ServiceDescriptor(typeof(IAssemblyScanner), typeof(DefaultAssemblyScanner), serviceLifetime))
                        .AddService(new ServiceDescriptor(typeof(IPluginTypeSelector), typeof(DefaultPluginTypeSelector), serviceLifetime))
                        .AddService(new ServiceDescriptor(typeof(IAssemblyLoader), typeof(DefaultAssemblyLoader), serviceLifetime))
                        .AddService(new ServiceDescriptor(typeof(IPluginActivator), (sp) => new DefaultPluginActivator(), serviceLifetime))
                        .AddService(new ServiceDescriptor(typeof(IParameterConverter), typeof(JsonSerializerParameterConverter), serviceLifetime))
                        .AddService(new ServiceDescriptor(typeof(IResultConverter), typeof(JsonSerializerResultConverter), serviceLifetime))
                        .AddService(new ServiceDescriptor(typeof(T), (sp) =>
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

                            if (scanResults.Count() > 1)
                                throw new PriseDependencyInjectionException($"More than 1 plugin assembly was found for plugin type {typeof(T).Name}");

                            var scanResult = scanResults.First();
                            var pluginLoadContext = PluginLoadContext.DefaultPluginLoadContext(Path.Combine(scanResult.AssemblyPath, scanResult.AssemblyName), typeof(T), frameworkFromHost);
                            pluginLoadContext.IgnorePlatformInconsistencies = ignorePlatormInconsistencies;

                            IServiceCollection hostServicesCollection;
                            pluginLoadContext
                                .AddHostServices(services, out hostServicesCollection, includeHostServices, excludeHostServices ?? Enumerable.Empty<Type>())
                                .AddHostAssemblies(hostAssemblies)
                                .AddRemoteTypes(remoteTypes)
                                .AddDowngradableTypes(downgradableTypes)
                                .AddDowngradableHostAssemblies(downgradableHostAssemblies)
                                .AddAdditionalProbingPaths(additionalProbingPaths)
                            ;

                            if (hostServices != null)
                            {
                                hostServices.Invoke(hostServicesCollection);

                                foreach (var hostService in hostServicesCollection)
                                    pluginLoadContext
                                        // A host type will always live inside the host
                                        .AddHostTypes(new[] { hostService.ServiceType })
                                        // The implementation type will always exist on the Host, since it will be created here
                                        .AddHostTypes(new[] { hostService.ImplementationType ?? hostService.ImplementationInstance?.GetType() ?? hostService.ImplementationFactory?.Method.ReturnType });
                                ;
                            }

                            var sharedServicesCollection = new ServiceCollection();
                            if (sharedServices != null)
                            {
                                sharedServices.Invoke(sharedServicesCollection);

                                foreach (var sharedService in sharedServicesCollection)
                                    pluginLoadContext
                                        // The service type must exist on the remote to support backwards compatability
                                        .AddRemoteTypes(new[] { sharedService.ServiceType })
                                        // If a shared service is added, it must be a added as a host type
                                        // The implementation type will always exist on the Host, since it will be created here
                                        .AddHostTypes(new[] { sharedService.ImplementationType ?? sharedService.ImplementationInstance?.GetType() ?? sharedService.ImplementationFactory?.Method.ReturnType })
                                    ;
                            }

                            var pluginAssembly = loader.Load(pluginLoadContext).Result;
                            var pluginTypes = selector.SelectPluginTypes<T>(pluginAssembly);
                            var firstPlugin = pluginTypes.FirstOrDefault();

                            return activator.ActivatePlugin<T>(new Activation.DefaultPluginActivationOptions
                            {
                                PluginType = firstPlugin,
                                PluginAssembly = pluginAssembly,
                                ParameterConverter = parameterConverter,
                                ResultConverter = resultConverter,
                                HostServices = hostServicesCollection,
                                SharedServices = sharedServicesCollection
                            }).Result;
                        }, serviceLifetime))
            ;
        }


        public static IServiceCollection AddPrisePlugins<T>(this IServiceCollection services,
                                                            string pathToPlugins,
                                                            bool ignorePlatormInconsistencies = false,
                                                            string hostFramework = null,
                                                            IEnumerable<Type> includeHostServices = null,
                                                            IEnumerable<Type> excludeHostServices = null,
                                                            Action<IServiceCollection> hostServices = null,
                                                            Action<IServiceCollection> sharedServices = null,
                                                            IEnumerable<string> hostAssemblies = null,
                                                            IEnumerable<Type> remoteTypes = null,
                                                            IEnumerable<Type> downgradableTypes = null,
                                                            IEnumerable<string> downgradableHostAssemblies = null,
                                                            IEnumerable<string> additionalProbingPaths = null)
                   where T : class
        {
            var serviceLifetime = ServiceLifetime.Scoped;

            return services
                       .AddService(new ServiceDescriptor(typeof(IAssemblyScanner), typeof(DefaultAssemblyScanner), serviceLifetime))
                       .AddService(new ServiceDescriptor(typeof(IPluginTypeSelector), typeof(DefaultPluginTypeSelector), serviceLifetime))

                       .AddService(new ServiceDescriptor(typeof(IAssemblyLoader), typeof(DefaultAssemblyLoader), ServiceLifetime.Transient)) // request a new one for each assembly

                       .AddService(new ServiceDescriptor(typeof(IPluginActivator), (sp) => new DefaultPluginActivator(), serviceLifetime))
                       .AddService(new ServiceDescriptor(typeof(IParameterConverter), typeof(JsonSerializerParameterConverter), serviceLifetime))
                       .AddService(new ServiceDescriptor(typeof(IResultConverter), typeof(JsonSerializerResultConverter), serviceLifetime))
                       .AddService(new ServiceDescriptor(typeof(IEnumerable<T>), (sp) =>
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

                               IServiceCollection hostServicesCollection;
                               pluginLoadContext
                                   .AddHostServices(services, out hostServicesCollection, includeHostServices, excludeHostServices ?? Enumerable.Empty<Type>())
                                   .AddHostAssemblies(hostAssemblies)
                                   .AddRemoteTypes(remoteTypes)
                                   .AddDowngradableTypes(downgradableTypes)
                                   .AddDowngradableHostAssemblies(downgradableHostAssemblies)
                                   .AddAdditionalProbingPaths(additionalProbingPaths)
                               ;

                               if (hostServices != null)
                               {
                                   hostServices.Invoke(hostServicesCollection);

                                   foreach (var hostService in hostServicesCollection)
                                       pluginLoadContext
                                           // A host type will always live inside the host
                                           .AddHostTypes(new[] { hostService.ServiceType })
                                           // The implementation type will always exist on the Host, since it will be created here
                                           .AddHostTypes(new[] { hostService.ImplementationType ?? hostService.ImplementationInstance?.GetType() ?? hostService.ImplementationFactory?.Method.ReturnType });
                                   ;
                               }

                               var sharedServicesCollection = new ServiceCollection();
                               if (sharedServices != null)
                               {
                                   sharedServices.Invoke(sharedServicesCollection);

                                   foreach (var sharedService in sharedServicesCollection)
                                       pluginLoadContext
                                           // The service type must exist on the remote to support backwards compatability
                                           .AddRemoteTypes(new[] { sharedService.ServiceType })
                                           // If a shared service is added, it must be a added as a host type
                                           // The implementation type will always exist on the Host, since it will be created here
                                           .AddHostTypes(new[] { sharedService.ImplementationType ?? sharedService.ImplementationInstance?.GetType() ?? sharedService.ImplementationFactory?.Method.ReturnType })
                                       ;
                               }

                               var pluginAssembly = loader.Load(pluginLoadContext).Result;
                               var pluginTypes = selector.SelectPluginTypes<T>(pluginAssembly);
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
        private static IServiceCollection AddService(this IServiceCollection services, ServiceDescriptor serviceDescriptor)
        {
            services
               .Add(serviceDescriptor);
            return services;
        }
    }
}