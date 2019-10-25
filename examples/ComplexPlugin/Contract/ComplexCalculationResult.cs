using System;

namespace Contract
{
    // Plugin result types ARE required to be serializable
    [Serializable]
    public class ComplexCalculationResult
    {
        public CalculationResult[] Results { get; set; }
    }
}