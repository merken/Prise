using System;
using Prise.Plugin;

namespace Prise.Activation
{
    public class PluginService
    {
        public string FieldName { get; set; }
        public Type ServiceType { get; set; }
        public ProvidedBy ProvidedBy { get; set; }
        public Type BridgeType { get; set; }
    }
}