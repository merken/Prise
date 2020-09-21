using System;
using Microsoft.Extensions.DependencyInjection;

namespace Prise.Plugin
{
    [Obsolete("Usage of a PluginBootstrapper is obsolete in favor of field injection using the " + nameof(PluginServiceAttribute) + ". Existing plugins will continue to function as normal.", false)]
    public interface IPluginBootstrapper 
    {
        IServiceCollection Bootstrap(IServiceCollection services);
    }
}