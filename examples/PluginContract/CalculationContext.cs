using System;

namespace PluginContract
{
    [Serializable]
    public class CalculationContext
    {
        public decimal A { get; set; }
        public decimal B { get; set; }
    }
}