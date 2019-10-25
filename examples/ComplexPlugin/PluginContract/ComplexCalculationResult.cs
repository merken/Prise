using System;

namespace PluginContract
{
    // Plugin result types ARE required to be serializable
    [Serializable]
    public class ComplexCalculationResult
    {
        public CalculationResult[] Results { get; set; }
    }
}