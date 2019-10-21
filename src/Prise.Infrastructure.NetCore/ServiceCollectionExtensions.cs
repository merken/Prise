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
        public static IServiceCollection AddPrise<T>(this IServiceCollection services, Action<PluginLoadOptionsBuilder<T>> config = null)
            where T : class
        {
            return services.AddPriseWithPluginLoader<T, PrisePluginLoader<T>>(config);
        }

        public static IServiceCollection AddPriseWithPluginLoader<T, TPluginLoader>(this IServiceCollection services, Action<PluginLoadOptionsBuilder<T>> config = null)
            where T : class
            where TPluginLoader : class, IPluginLoader<T>
        {
            var optionsBuilder = new PluginLoadOptionsBuilder<T>().WithDefaultOptions();
            config?.Invoke(optionsBuilder);

            services = optionsBuilder.RegisterOptions(services);

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