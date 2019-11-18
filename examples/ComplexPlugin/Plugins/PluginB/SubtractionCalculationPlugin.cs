using System;
using System.Collections.Generic;
using System.Linq;
using Contract;
using Prise.Plugin;

namespace PluginB
{
    // This class does not implement the ICalculationPlugin interface
    // Since all the methods are present, it will continue to work because the PluginAttribute is still present and the interface contract is respected
    // This improves backwards compatibility.
    [Plugin(PluginType = typeof(ICalculationPlugin))]
    public class SubtractionCalculationPlugin
    {
        public string Name => nameof(SubtractionCalculationPlugin);

        // Property Description will not be implemented in this plugin, all other methods can still be called
        // public string Description => "This plugin performs subtraction";
        // However, you could expose it this way:
        // public string get_Description() => "This plugin performs subtraction";
        public int Calculate(int a, int b)
        {
            return a - b;
        }

        public decimal Calculate(decimal a, decimal b)
        {
            return a - b;
        }

        public decimal CalculateComplex(CalculationContext context)
        {
            return context.A - context.B;
        }

        public CalculationResult CalculateComplexResult(CalculationContext context)
        {
            return new CalculationResult { Result = context.A - context.B };
        }

        public ComplexCalculationResult CalculateMutiple(ComplexCalculationContext context)
        {
            var results = new List<CalculationResult>();
            results.AddRange(context.Calculations.Select(c => new CalculationResult { Result = c.A - c.B }));
            return new ComplexCalculationResult
            {
                Results = results.ToArray()
            };
        }
    }
}
