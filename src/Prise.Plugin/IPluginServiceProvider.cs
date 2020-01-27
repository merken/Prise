using System;

namespace Prise.Plugin
{
    public interface IPluginServiceProvider
    {
        object GetPluginService(Type type);
        object GetHostService(Type type);
    }
}
