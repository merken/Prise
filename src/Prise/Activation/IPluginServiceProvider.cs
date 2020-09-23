using System;

namespace Prise.Activation
{
    public interface IBootstrapperServiceProvider : IDisposable
    {
        object GetHostService(Type type);
    }

    public interface IPluginServiceProvider : IBootstrapperServiceProvider
    {
        object GetPluginService(Type type);
    }
}