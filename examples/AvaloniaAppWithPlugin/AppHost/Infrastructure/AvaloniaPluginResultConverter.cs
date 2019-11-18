using System;
using Prise;

namespace AppHost.Infrastructure
{
    public class AvaloniaPluginResultConverter : ResultConverter
    {
        public override object Deserialize(Type localType, Type remoteType, object value)
        {
            // No conversion, no backwards compatibility
            // When the host upgrades any Avalonia dependency, it will break
            return value;
        }
    }
}