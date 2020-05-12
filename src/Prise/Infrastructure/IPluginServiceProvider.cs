using System;

namespace Prise.Infrastructure
{
    public interface IPluginServiceProvider : IDisposable
    {
        object GetPluginService(Type type);
        object GetHostService(Type type);
    }
}
