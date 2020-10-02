using System;
using Microsoft.Extensions.DependencyInjection;
using Prise.Plugin;

namespace Prise.Activation
{
    public interface IRemotePluginActivator : IDisposable
    {
        object CreateRemoteBootstrapper(IPluginActivationContext pluginActivationContext, IServiceCollection hostServices = null);
        object CreateRemoteInstance(IPluginActivationContext pluginActivationContext, IPluginBootstrapper bootstrapper = null, IServiceCollection hostServices = null);
    }
}