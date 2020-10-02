using System;
using Microsoft.Extensions.DependencyInjection;
using Prise.Proxy;

namespace Prise.Activation
{
    public class DefaultPluginActivationOptions : IPluginActivationOptions
    {
        public IAssemblyShim PluginAssembly { get; set; }
        public Type PluginType { get; set; }
        public IServiceCollection HostServices { get; set; }
        public IParameterConverter ParameterConverter { get; set; }
        public IResultConverter ResultConverter { get; set; }
    }
}