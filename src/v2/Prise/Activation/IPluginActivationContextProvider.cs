using System;

namespace Prise.Activation
{
    public interface IPluginActivationContextProvider
    {
        IPluginActivationContext ProvideActivationContext(Type remoteType, IAssemblyShim pluginAssembly);
    }
}