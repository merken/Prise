using System;
using Microsoft.Extensions.DependencyInjection;
using Prise.Plugin;

namespace Prise.Activation
{
    public interface IRemotePluginActivator : IDisposable
    {
        object CreateRemoteBootstrapper(Type bootstrapperType, IAssemblyShim assembly);
        object CreateRemoteInstance(IPluginActivationContext pluginActivationContext, IPluginBootstrapper bootstrapper = null, IServiceCollection sharedServices = null, IServiceCollection hostServices = null);
    }
}