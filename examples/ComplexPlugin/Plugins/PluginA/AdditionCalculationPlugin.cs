using System.Collections.Generic;
using System.Linq;
using Contract;
using Prise.Plugin;

namespace PluginA
{
    /// <summary>
    /// This plugin does not require any 3rd party dependencies or dependency injection, 
    ///     as long as a default parameterless constructor is present (implicitly or explicitly), this plugin will get loaded.
    /// </summary>
    [Plugin(PluginType = typeof(ICalculationPlugin))]
    public class AdditionCalculationPlugin : ICalculationPlugin
    {
        public string Name => nameof(AdditionCalculationPlugin);
        public string Description => "This plugin performs addition";
        public int Calculate(int a, int b)
        {
            return a + b;
        }

        public decimal Calculate(decimal a, decimal b)
        {
            return a + b;
        }

        public decimal CalculateComplex(CalculationContext context)
        {
            return context.A + context.B;
        }

        public CalculationResult CalculateComplexResult(CalculationContext context)
        {
            return new CalculationResult
            {
                Result = context.A + context.B
            };
        }

        public ComplexCalculationResult CalculateMutiple(ComplexCalculationContext context)
        {
            var results = new List<CalculationResult>();
            results.AddRange(context.Calculations.Select(c => new CalculationResult { Result = c.A + c.B }));
            return new ComplexCalculationResult
            {
                Results = results.ToArray()
            };
        }
    }
}
