using System;

namespace PluginContract
{
    public interface ICalculationPlugin
    {
        int Calculate(int a, int b);
        decimal Calculate(decimal a, decimal b);
        decimal CalculateComplex(CalculationContext context);
        CalculationResult CalculateComplexResult(CalculationContext context);
        CalculationResult CalculateMutiple(ComplexCalculationContext context);
    }
}
