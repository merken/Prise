using System;
using System.Collections.Generic;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;

namespace Prise.Infrastructure.NetCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPrise<T>(this IServiceCollection services, Action<PluginLoadOptionsBuilder<T>> config = null)
            where T : class//, IDisposable
        {
            return services.AddPriseWithPluginLoader<T, PrisePluginLoader<T>>(config);
        }

        public static IServiceCollection AddPriseWithPluginLoader<T, TPluginLoader>(this IServiceCollection services, Action<PluginLoadOptionsBuilder<T>> config = null)
            where T : class//, IDisposable // todo
            where TPluginLoader : class, IPluginLoader<T>, IPluginResolver<T>
        {
            var optionsBuilder = new PluginLoadOptionsBuilder<T>().WithDefaultOptions();
            config?.Invoke(optionsBuilder);

            services = optionsBuilder.RegisterOptions(services);

            return services
                .AddScoped<IPluginLoader<T>, TPluginLoader>()
                .AddScoped<IPluginResolver<T>, TPluginLoader>()
                .AddScoped<T>((s) =>
                {
                    // Synchronous plugin loading
                    return s.GetRequiredService<IPluginResolver<T>>().Load();
                })
                .AddScoped<IEnumerable<T>>((s) =>
                {
                    // Synchronous plugins loading
                    return s.GetRequiredService<IPluginResolver<T>>().LoadAll();
                });
        }
    }
}