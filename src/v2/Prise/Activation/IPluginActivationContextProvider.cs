using System;

namespace Prise.Activation
{
    public interface IPluginActivationDescriptorProvider
    {
        PluginActivationDescriptor ProvideActivationDescriptor(Type remoteType, IAssemblyShim pluginAssembly);
    }
}