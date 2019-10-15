using System;
using System.Collections.Generic;
using System.Linq;
using Domain;
using PluginC.Calculations;
using PluginContract;
using Prise.Infrastructure;

namespace PluginC
{
    // This plugin will Divide or Multiple, who knows, it's always a guess.
    // The decision is made in a service, which dependend a discount from a third party dependency that no other plugin shares
    [Plugin(PluginType = typeof(ICalculationPlugin))]
    public class DivideOrMultiplyCalculationPlugin : ICalculationPlugin
    {
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

        [PluginFactory]
        public static ICalculationPlugin ThisNameDoesNotMatterFactoryMethod(IServiceProvider serviceProvider)
        {
            return new DivideOrMultiplyCalculationPlugin((ICanCalculate)serviceProvider.GetService(typeof(ICanCalculate)));
        }
    }
}
