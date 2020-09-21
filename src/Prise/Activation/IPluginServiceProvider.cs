using System;

namespace Prise.Activation
{
    public interface IPluginServiceProvider : IDisposable
    {
        object GetPluginService(Type type);
        object GetHostService(Type type);
    }
}