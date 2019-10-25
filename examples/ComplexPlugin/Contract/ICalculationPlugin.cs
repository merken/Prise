using System;

namespace Contract
{
    public interface ICalculationPlugin
    {
        string Name { get; }
        string Description { get; }
        int Calculate(int a, int b);
        decimal Calculate(decimal a, decimal b);
        decimal CalculateComplex(CalculationContext context);
        CalculationResult CalculateComplexResult(CalculationContext context);
        ComplexCalculationResult CalculateMutiple(ComplexCalculationContext context);
    }
}
