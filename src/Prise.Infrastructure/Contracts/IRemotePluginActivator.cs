using System;
using System.Reflection;

namespace Prise.Infrastructure
{
    public interface IRemotePluginActivator : IDisposable
    {
        object CreateRemoteBootstrapper(Type bootstrapperType, Assembly assembly);
        object CreateRemoteInstance(Type pluginType, IPluginBootstrapper bootstrapper, MethodInfo factoryMethod, Assembly assembly);
    }
}