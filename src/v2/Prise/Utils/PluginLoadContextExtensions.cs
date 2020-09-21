using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Prise.Core;

namespace Prise.Utils
{
    public static class PluginLoadContextExtensions
    {
        public static PluginLoadContext AddHostServices(this PluginLoadContext pluginLoadContext,
               IServiceCollection hostServices,
               out IServiceCollection services,
               IEnumerable<Type> includeTypes = null,
               IEnumerable<Type> excludeTypes = null)
        {
            services = new ServiceCollection();

            if (includeTypes == null || !includeTypes.Any())
                return pluginLoadContext; // short circuit

            var hostTypes = new List<Type>();
            var priseServices = hostServices.Where(s => IsPriseService(s.ServiceType));
            var includeServices = hostServices.Where(s => Includes(s.ServiceType, includeTypes));
            var excludeServices = hostServices.Where(s => Excludes(s.ServiceType, excludeTypes));

            foreach (var hostService in hostServices
                                        .Except(priseServices)
                                        .Union(includeServices)
                                        .Except(excludeServices))
                pluginLoadContext.AddHostService(
                    services,
                    hostService);

            return pluginLoadContext;
        }

        public static PluginLoadContext AddHostService(this PluginLoadContext pluginLoadContext, IServiceCollection services, Type hostServiceType, Type hostServiceImplementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            return pluginLoadContext.AddHostService(services, new ServiceDescriptor(hostServiceType, hostServiceImplementationType, serviceLifetime));
        }
        
        public static PluginLoadContext AddHostService(this PluginLoadContext pluginLoadContext, IServiceCollection services, ServiceDescriptor hostService)
        {
            // Add the Host service to the servicecollection of the plugin
            services.Add(hostService);

            return pluginLoadContext
                  // A host type will always live inside the host
                  .AddHostTypes(new[] { hostService.ServiceType })
                  // The implementation type will always exist on the Host, since it will be created here
                  .AddHostTypes(new[] { hostService.ImplementationType ?? hostService.ImplementationInstance?.GetType() ?? hostService.ImplementationFactory?.Method.ReturnType });
        }

        public static PluginLoadContext AddHostTypes(this PluginLoadContext pluginLoadContext, IEnumerable<Type> hostTypes)
        {
            if (hostTypes == null || !hostTypes.Any())
                return pluginLoadContext; // short circuit

            pluginLoadContext.HostTypes = new List<Type>(pluginLoadContext.HostTypes.Union(hostTypes));
            return pluginLoadContext;
        }

        public static PluginLoadContext AddHostAssemblies(this PluginLoadContext pluginLoadContext, IEnumerable<string> assemblies)
        {
            if (assemblies == null || !assemblies.Any())
                return pluginLoadContext; // short circuit

            pluginLoadContext.HostAssemblies = new List<string>(pluginLoadContext.HostAssemblies.Union(assemblies));
            return pluginLoadContext;
        }

        public static PluginLoadContext AddRemoteTypes(this PluginLoadContext pluginLoadContext, IEnumerable<Type> remoteTypes)
        {
            if (remoteTypes == null || !remoteTypes.Any())
                return pluginLoadContext; // short circuit

            pluginLoadContext.RemoteTypes = new List<Type>(pluginLoadContext.RemoteTypes.Union(remoteTypes));
            return pluginLoadContext;
        }

        public static PluginLoadContext AddDowngradableTypes(this PluginLoadContext pluginLoadContext, IEnumerable<Type> downgradableTypes)
        {
            if (downgradableTypes == null || !downgradableTypes.Any())
                return pluginLoadContext; // short circuit

            pluginLoadContext.DowngradableTypes = new List<Type>(pluginLoadContext.DowngradableTypes.Union(downgradableTypes));
            return pluginLoadContext;
        }

        public static PluginLoadContext AddDowngradableHostAssemblies(this PluginLoadContext pluginLoadContext, IEnumerable<string> assemblies)
        {
            if (assemblies == null || !assemblies.Any())
                return pluginLoadContext; // short circuit

            pluginLoadContext.DowngradableHostAssemblies = new List<string>(pluginLoadContext.DowngradableHostAssemblies.Union(assemblies));
            return pluginLoadContext;
        }

        public static PluginLoadContext AddAdditionalProbingPaths(this PluginLoadContext pluginLoadContext, IEnumerable<string> additionalProbingPaths)
        {
            if (additionalProbingPaths == null || !additionalProbingPaths.Any())
                return pluginLoadContext; // short circuit

            pluginLoadContext.AdditionalProbingPaths = new List<string>(pluginLoadContext.AdditionalProbingPaths.Union(additionalProbingPaths));
            return pluginLoadContext;
        }

        public static PluginLoadContext SetNativeDependencyLoadPreference(this PluginLoadContext pluginLoadContext, NativeDependencyLoadPreference nativeDependencyLoadPreference)
        {
            pluginLoadContext.NativeDependencyLoadPreference = nativeDependencyLoadPreference;
            return pluginLoadContext;
        }

        public static PluginLoadContext SetPlatformVersion(this PluginLoadContext pluginLoadContext, PluginPlatformVersion pluginPlatformVersion)
        {
            pluginLoadContext.PluginPlatformVersion = pluginPlatformVersion;
            return pluginLoadContext;
        }

        public static PluginLoadContext SetRuntimePlatformContext(this PluginLoadContext pluginLoadContext, IRuntimePlatformContext runtimePlatformContext)
        {
            pluginLoadContext.RuntimePlatformContext = runtimePlatformContext;
            return pluginLoadContext;
        }

        private static bool IsPriseService(Type type) => type.Namespace.StartsWith("Prise.");

        private static bool Includes(Type type, IEnumerable<Type> includeTypes)
        {
            if (includeTypes == null)
                return true;
            return includeTypes.Contains(type);
        }

        private static bool Excludes(Type type, IEnumerable<Type> excludeTypes)
        {
            if (excludeTypes == null)
                return false;
            return excludeTypes.Contains(type);
        }
    }
}