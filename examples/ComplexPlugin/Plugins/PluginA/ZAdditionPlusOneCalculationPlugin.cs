using System.Collections.Generic;
using System.Linq;
using Contract;
using Prise.Plugin;

namespace PluginA
{
    /// <summary>
    /// By default, only the first plugin is loaded, the first in alphabethical order
    /// In order to execute the ZAdditionPlusOneCalculationPlugin, you need to have an IEnumerable<ICalculationPlugin> injected
    ///     or inject a IPluginLoader<ICalculationPlugin> and call the .LoadAll() method.
    /// </summary>
    [Plugin(PluginType = typeof(ICalculationPlugin))]
    public class ZAdditionPlusOneCalculationPlugin : ICalculationPlugin
    {
        public string Name => nameof(ZAdditionPlusOneCalculationPlugin);
        public string Description => "This plugin performs addition +1";
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