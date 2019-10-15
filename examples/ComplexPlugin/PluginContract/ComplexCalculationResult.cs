using System;

namespace PluginContract
{
    [Serializable]
    public class ComplexCalculationResult
    {
        public CalculationResult[] Results { get; set; }
    }
}