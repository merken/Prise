using System;

namespace Prise.Infrastructure
{
    public interface IPluginServiceProvider
    {
        object GetPluginService(Type type);
        object GetHostService(Type type);
    }
}
