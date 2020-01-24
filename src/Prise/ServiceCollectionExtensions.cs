using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Prise.Infrastructure;

namespace Prise
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPrise<T>(this IServiceCollection services,
            Action<PluginLoadOptionsBuilder<T>> config = null,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
            where T : class
        {
            return services.AddPriseWithPluginLoader<T, PrisePluginLoader<T>>(config, serviceLifetime);
        }

        public static IServiceCollection AddPriseAsSingleton<T>(this IServiceCollection services,
            Action<PluginLoadOptionsBuilder<T>> config = null)
            where T : class
        {
            return services.AddPrise<T>(config, ServiceLifetime.Singleton);
        }

        public static IServiceCollection AddPriseWithPluginLoader<T, TPluginLoader>(
                this IServiceCollection services,
                Action<PluginLoadOptionsBuilder<T>> config = null,
                ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
            where T : class
            where TPluginLoader : class, IPluginLoader<T>, IPluginResolver<T>
        {
            var optionsBuilder = new PluginLoadOptionsBuilder<T>()
                .WithDefaultOptions(serviceLifetime: serviceLifetime);
            config?.Invoke(optionsBuilder);

            services = optionsBuilder.RegisterOptions(services);

            return services
                .AddService(new ServiceDescriptor(typeof(IPluginLoader<T>), typeof(TPluginLoader), serviceLifetime))
                .AddService(new ServiceDescriptor(typeof(IPluginResolver<T>), typeof(TPluginLoader), serviceLifetime))
                .AddService(new ServiceDescriptor(typeof(T), (s) =>
                {
                    // Synchronous plugin loading
                    return s.GetRequiredService<IPluginResolver<T>>().Load();
                }, serviceLifetime))
                .AddService(new ServiceDescriptor(typeof(IEnumerable<T>), (s) =>
                {
                    // Synchronous plugin loading
                    return s.GetRequiredService<IPluginResolver<T>>().LoadAll();
                }, serviceLifetime));
        }

        private static IServiceCollection AddService(this IServiceCollection services, ServiceDescriptor serviceDescriptor)
        {
            services
               .Add(serviceDescriptor);
            return services;
        }
    }
}