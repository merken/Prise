using System;

namespace Prise.Plugin
{
    public interface IPluginServiceProvider
    {
        T GetPluginService<T>();
        T GetHostService<T>();
        object GetSharedHostService(Type type);
    }
}
