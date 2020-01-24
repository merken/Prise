using System;
using System.Reflection;

namespace Prise.Infrastructure
{
    public class PluginActivationContext
    {
        public Assembly PluginAssembly { get; set; }
        public Type PluginType { get; set; }
        public Type PluginBootstrapperType { get; set; }
        public MethodInfo PluginFactoryMethod { get; set; }
        public MethodInfo PluginFactoryMethodWithPluginServiceProvider { get; set; }
    }
}
