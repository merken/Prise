using System;
using System.Collections.Generic;
using System.Linq;
using Prise.IntegrationTestsContract;
using PluginCFromNetwork.Calculations;
using Prise.Plugin;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace PluginCFromNetwork
{
    // This plugin will be loaded from the network
    [Plugin(PluginType = typeof(INetworkCalculationPlugin))]
    public class DivideOrMultiplyCalculationPlugin : INetworkCalculationPlugin
    {
        public string Name => nameof(DivideOrMultiplyCalculationPlugin);

        public string Description => $"This plugin performs division OR multiplication, check out {nameof(DivideOrMultiplyCalculationBootstrapper)} for more details";
        private readonly ICanCalculate calculation;

        internal DivideOrMultiplyCalculationPlugin(ICanCalculate calculation)
        {
            this.calculation = calculation;
        }

        public int Calculate(int a, int b)
        {
            return (int)this.calculation.DoCalculation(a, b);
        }

        public decimal Calculate(decimal a, decimal b)
        {
            return this.calculation.DoCalculation(a, b);
        }

        public decimal CalculateComplex(CalculationContext context)
        {
            return this.calculation.DoCalculation(context.A, context.B);
        }

        public CalculationResult CalculateComplexResult(CalculationContext context)
        {
            return new CalculationResult { Result = this.calculation.DoCalculation(context.A, context.B) };
        }

        public ComplexCalculationResult CalculateMutiple(ComplexCalculationContext context)
        {
            var results = new List<CalculationResult>();
            results.AddRange(context.Calculations.Select(c => new CalculationResult { Result = this.calculation.DoCalculation(c.A, c.B) }));
            return new ComplexCalculationResult
            {
                Results = results.ToArray()
            };
        }

        public async Task<ComplexCalculationResult> CalculateMutipleAsync(ComplexCalculationContext context)
        {
            var results = new List<CalculationResult>();
            results.AddRange(context.Calculations.Select(c => new CalculationResult { Result = this.calculation.DoCalculation(c.A, c.B) }));

            await Task.Delay(2500);

            return new ComplexCalculationResult
            {
                Results = results.ToArray()
            };
        }

        [PluginFactory]
        public static INetworkCalculationPlugin ThisNameDoesNotMatterFactoryMethod(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var serviceConfig = config["DivideOrMultiply"];

            // Since multiple ICanCalculate services are registered in the DivideOrMultiplyCalculationBootstrapper, multiple can be resolved
            var services = serviceProvider.GetServices<ICanCalculate>();

            // Choose the correct service based on the name
            var serviceToUse = services.First(s => s.ServiceName == serviceConfig);

            return new DivideOrMultiplyCalculationPlugin(serviceToUse);
        }
    }
}
