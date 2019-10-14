using System;
using System.Reflection;

namespace Prise.Infrastructure
{
    public interface IRemotePluginActivator
    {
        object CreateRemoteBootstrapper(Type boostrapperType);
        object CreateRemoteInstance(Type pluginType, IPluginBootstrapper bootstrapper, MethodInfo factoryMethod);
    }
}