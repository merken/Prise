using System;

namespace PluginContract
{
    [Serializable]
    public class ComplexCalculationContext
    {
        public CalculationContext[] Calculations { get; set; }
    }
}