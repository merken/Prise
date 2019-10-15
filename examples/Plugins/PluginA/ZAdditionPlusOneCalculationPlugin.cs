using System.Collections.Generic;
using System.Linq;
using PluginContract;
using Prise.Infrastructure;

namespace PluginA
{
    /// <summary>
    /// This plugin can only be loaded using the SupportMultiplePlugins option.
    /// Plugins are loaded using an alphabetical order. The SinglePluginLoader will not return this instance.
    /// </summary>
    [Plugin(PluginType = typeof(ICalculationPlugin))]
    public class ZAdditionPlusOneCalculationPlugin : ICalculationPlugin
    {
        public int Calculate(int a, int b)
        {
            return a + b + 1;
        }

        public decimal Calculate(decimal a, decimal b)
        {
            return a + b + 1;
        }

        public decimal CalculateComplex(CalculationContext context)
        {
            return context.A + context.B + 1;
        }

        public CalculationResult CalculateComplexResult(CalculationContext context)
        {
            return new CalculationResult
            {
                Result = context.A + context.B + 1
            };
        }

        public ComplexCalculationResult CalculateMutiple(ComplexCalculationContext context)
        {
            var results = new List<CalculationResult>();
            results.AddRange(context.Calculations.Select(c => new CalculationResult { Result = c.A + c.B + 1 }));
            return new ComplexCalculationResult
            {
                Results = results.ToArray()
            };
        }
    }
}