using System;
using DomainForPluginC;
using Microsoft.Extensions.DependencyInjection;
using PluginC.Calculations;
using Prise.Plugin;

namespace PluginC
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

            // Randomly choose what service to use
            // var random = new Random();
            // if (random.Next() % 2 == 0)
            //     services.AddScoped<ICanCalculate, DivideCalculation>();
            // else
            //     services.AddScoped<ICanCalculate, MultiplyCalculation>();

            services.AddScoped<ICanCalculate, MultiplyCalculation>();
            
            return services;
        }
    }
}