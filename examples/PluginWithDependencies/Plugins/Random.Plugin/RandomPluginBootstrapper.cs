using System;
using Contract;
using Microsoft.Extensions.DependencyInjection;
using Prise.Plugin;
using Random.Domain;

namespace Random.Plugin
{
    /// <summary>
    /// This bootstrapper will be used in order to load the RandomPlugin
    /// </summary>
    [PluginBootstrapper(PluginType = typeof(RandomPlugin))]
    public class RandomPluginBootstrapper : IPluginBootstrapper
    {
        public IServiceCollection Bootstrap(IServiceCollection services)
        {
            // Register all services that are required to load the RandomPlugin
            services.AddScoped<IRandomService, RandomService>();
            return services;
        }
    }
}
