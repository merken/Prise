using System;

namespace Prise.Plugin
{
    [Obsolete("Usage of a PluginBootstrapper is obsolete, please use field injection instead. Existing plugins will continue to function as normal.", false)]
    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class PluginFactoryAttribute : System.Attribute { }
}