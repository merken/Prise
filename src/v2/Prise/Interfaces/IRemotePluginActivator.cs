using System;
using Prise.Plugin;

namespace Prise.V2
{
    public interface IRemotePluginActivator : IDisposable
    {
        object CreateRemoteBootstrapper(Type bootstrapperType, IAssemblyShim assembly);
        object CreateRemoteInstance(PluginActivationContext pluginActivationContext, IPluginBootstrapper bootstrapper = null);
    }
}