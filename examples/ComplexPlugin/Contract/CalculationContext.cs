using System;

namespace Contract
{
    // Plugin Parameter types are not required to be serializable, Newtonsoft JSON takes care of this
    public class CalculationContext
    {
        public decimal A { get; set; }
        public decimal B { get; set; }
    }
}