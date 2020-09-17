using System;

namespace Prise.V2
{
    public interface IPluginServiceProvider : IDisposable
    {
        object GetPluginService(Type type);
        object GetHostService(Type type);
    }
}