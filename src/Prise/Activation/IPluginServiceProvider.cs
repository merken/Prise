using System;

namespace Prise.Activation
{
    public interface IPluginServiceProvider : IBootstrapperServiceProvider
    {
        object GetPluginService(Type type);
    }
}