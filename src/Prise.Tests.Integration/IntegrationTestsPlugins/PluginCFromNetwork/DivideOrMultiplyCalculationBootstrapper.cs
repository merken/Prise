using System;
using DomainForPluginC;
using Microsoft.Extensions.DependencyInjection;
using PluginCFromNetwork.Calculations;
using Prise.Plugin;

namespace PluginCFromNetwork
{
    [PluginBootstrapper(PluginType = typeof(DivideOrMultiplyCalculationPlugin))]
    public class DivideOrMultiplyCalculationBootstrapper : IPluginBootstrapper
    {
        public IServiceCollection Bootstrap(IServiceCollection services)
        {
            // Discount and DiscountService come from a third party assembly called Domain
            // Add a fixed discount of 10%
            services.AddSingleton<IDiscount>(new Discount(1.10m));
            services.AddScoped<IDiscountService, DiscountService>();

            services.AddScoped<ICanCalculate, MultiplyCalculation>();
            services.AddScoped<ICanCalculate, DivideCalculation>();
            
            return services;
        }
    }
}