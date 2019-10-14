using System;
using System.Linq;
using PluginContract;

namespace PluginA
{
    /// <summary>
    /// This plugin does not require any 3rd party dependencies or dependency injection, 
    /// as long as a default parameterless constructor is present (implicitly or explicitly) this plugin will get loaded.
    /// </summary>
    public class AdditionCalculationPlugin : ICalculationPlugin
    {
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
            return new CalculationResult{
                Result = context.A+context.B
            };
        }

        public CalculationResult CalculateMutiple(ComplexCalculationContext context)
        {
            return new CalculationResult{
                
                Result = context.Calculations.Sum(ctx => ctx.A + ctx.B)
            };
        }
    }
}
