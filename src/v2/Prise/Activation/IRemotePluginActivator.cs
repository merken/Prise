using System;
using Prise.Plugin;

namespace Prise.Activation
{
    public interface IRemotePluginActivator : IDisposable
    {
        object CreateRemoteBootstrapper(Type bootstrapperType, IAssemblyShim assembly);
        object CreateRemoteInstance(PluginActivationDescriptor pluginActivationContext, IPluginBootstrapper bootstrapper = null);
    }
}