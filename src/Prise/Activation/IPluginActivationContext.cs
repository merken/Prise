using System;
using System.Collections.Generic;
using System.Reflection;

namespace Prise.Activation
{
    public interface IPluginActivationContext
    {
        IAssemblyShim PluginAssembly { get; }
        Type PluginType { get; }
        Type PluginBootstrapperType { get; }
        MethodInfo PluginFactoryMethod { get; }
        MethodInfo PluginActivatedMethod { get; }
        IEnumerable<PluginService> PluginServices { get; }
        IEnumerable<BootstrapperService> BootstrapperServices { get; }
    }
}