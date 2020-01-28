using System;
using System.Collections.Generic;
using System.Reflection;
using Prise.Plugin;

namespace Prise.Infrastructure
{
    public class PluginService
    {
        public string FieldName { get; set; }
        public Type ServiceType { get; set; }
        public ProvidedBy ProvidedBy { get; set; }
        public Type BridgeType { get; set; }
    }

    public class PluginActivationContext
    {
        public Assembly PluginAssembly { get; set; }
        public Type PluginType { get; set; }
        public Type PluginBootstrapperType { get; set; }
        public MethodInfo PluginFactoryMethod { get; set; }
        public MethodInfo PluginActivatedMethod { get; set; }
        public IEnumerable<PluginService> PluginServices { get; set; }
    }
}
