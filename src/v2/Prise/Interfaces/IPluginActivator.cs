using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Prise.Proxy;

namespace Prise.V2
{
    public interface IPluginActivator
    {
        Task<T> ActivatePlugin<T>(IAssemblyShim pluginAssembly,
                                  Type pluginType = null,
                                  IServiceCollection sharedServices = null,
                                  IServiceCollection hostServices = null,
                                  IParameterConverter parameterConverter = null,
                                  IResultConverter resultConverter = null);
    }
}