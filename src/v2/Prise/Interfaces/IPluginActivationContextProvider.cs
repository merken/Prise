using System;

namespace Prise.V2
{
    public interface IPluginActivationContextProvider
    {
        PluginActivationContext ProvideActivationContext(Type remoteType, IAssemblyShim pluginAssembly);
    }
}