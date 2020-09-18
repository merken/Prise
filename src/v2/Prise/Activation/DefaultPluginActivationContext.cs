using System;
using System.Collections.Generic;
using System.Reflection;

namespace Prise.Activation
{
    public class DefaultPluginActivationContext : IPluginActivationContext
    {
        public IAssemblyShim PluginAssembly { get; set; }
        public Type PluginType { get; set; }
        public Type PluginBootstrapperType { get; set; }
        public MethodInfo PluginFactoryMethod { get; set; }
        public MethodInfo PluginActivatedMethod { get; set; }
        public IEnumerable<PluginService> PluginServices { get; set; }
    }
}