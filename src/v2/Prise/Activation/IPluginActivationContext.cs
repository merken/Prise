using System;
using Microsoft.Extensions.DependencyInjection;
using Prise.Proxy;

namespace Prise.Activation
{
    public interface IPluginActivationContext
    {
        IAssemblyShim pluginAssembly { get; }
        Type pluginType { get; }
        IServiceCollection sharedServices { get; }
        IServiceCollection hostServices { get; }
        IParameterConverter parameterConverter { get; }
        IResultConverter resultConverter { get; }
    }

    public class DefaultPluginActivationContext : IPluginActivationContext
    {
        public IAssemblyShim pluginAssembly { get; }
        public Type pluginType { get; }
        public IServiceCollection sharedServices { get; }
        public IServiceCollection hostServices { get; }
        public IParameterConverter parameterConverter { get; }
        public IResultConverter resultConverter { get; }
    }
}