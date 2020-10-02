using System;
using Microsoft.Extensions.DependencyInjection;
using Prise.Proxy;

namespace Prise.Activation
{
    public interface IPluginActivationOptions
    {
        IAssemblyShim PluginAssembly { get; }
        Type PluginType { get; }
        IServiceCollection HostServices { get; }
        IParameterConverter ParameterConverter { get; }
        IResultConverter ResultConverter { get; }
    }
}