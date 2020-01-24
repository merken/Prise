using System;
using System.Threading.Tasks;

namespace Prise.IntegrationTestsContract
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
        Task<ComplexCalculationResult> CalculateMutipleAsync(ComplexCalculationContext context);
    }
}
