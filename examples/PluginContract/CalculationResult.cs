using System;

namespace PluginContract
{
    // This object is deserialized using the Binary Result Converter by default
    // It must be serializable
    [Serializable]
    public class CalculationResult
    {
        public decimal Result { get; set; }
    }
}