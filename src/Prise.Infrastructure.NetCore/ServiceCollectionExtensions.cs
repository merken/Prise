using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Prise.Infrastructure.NetCore.Contracts;

namespace Prise.Infrastructure.NetCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPrise<T>(this IServiceCollection services, Action<PluggerOptionsBuilder<T>> config = null)
            where T : class
        {
            var optionsBuilder = new PluggerOptionsBuilder<T>().WithDefaultOptions();
            config?.Invoke(optionsBuilder);

            var options = optionsBuilder.Build();
            services.AddScoped<IPluginLoadOptions<T>>((s) => options);

            if (!options.SupportMultiplePlugins)
                AddSinglePluginLoader<T>(services);
            else
                AddMultiPluginLoader<T>(services);

            return services;
        }

        public static IServiceCollection AddPriseWithCustomLoader<T, TPluginLoader>(this IServiceCollection services, Action<PluggerOptionsBuilder<T>> config = null)
            where T : class
            where TPluginLoader : class, IPluginLoader<T>
        {
            var optionsBuilder = new PluggerOptionsBuilder<T>().WithDefaultOptions();

            config?.Invoke(optionsBuilder);
            var options = optionsBuilder.Build();

            services.AddScoped<IPluginLoadOptions<T>>((s) => options);

            return services
                .AddScoped<IPluginLoader<T>, TPluginLoader>()
                .AddScoped<T>((s) =>
                {
                    var loader = s.GetRequiredService<IPluginLoader<T>>();
                    return TryAndRethrowInnerException(loader.Load());
                })
                .AddScoped<IEnumerable<T>>((s) =>
                {
                    var loader = s.GetRequiredService<IPluginLoader<T>>();
                    return TryAndRethrowInnerException(loader.LoadAll());
                });
        }

        private static IServiceCollection AddSinglePluginLoader<T>(IServiceCollection services)
            where T : class
        {
            return services
                .AddScoped<IPluginLoader<T>, SinglePluginLoader<T>>()
                .AddScoped<T>((s) =>
                {
                    var loader = s.GetRequiredService<IPluginLoader<T>>();
                    return TryAndRethrowInnerException(loader.Load());
                });
        }

        private static IServiceCollection AddMultiPluginLoader<T>(IServiceCollection services)
            where T : class
        {
            return services
                .AddScoped<IPluginLoader<T>, MultiPluginLoader<T>>()
                .AddScoped<T>((s) =>
                {
                    var loader = s.GetRequiredService<IPluginLoader<T>>();
                    return TryAndRethrowInnerException(loader.Load());
                })
                .AddScoped<IEnumerable<T>>((s) =>
                {
                    var loader = s.GetRequiredService<IPluginLoader<T>>();
                    return TryAndRethrowInnerException(loader.LoadAll());
                });
        }

        private static T TryAndRethrowInnerException<T>(Task<T> task)
        {
            try
            {
                task.Wait();
                return task.Result;
            }
            catch (AggregateException ag)
            {
                if (ag.InnerException != null)
                    throw ag.InnerException;

                throw new NotSupportedException("Excpected inner exception to be present, but was null");
            }
        }
    }
}