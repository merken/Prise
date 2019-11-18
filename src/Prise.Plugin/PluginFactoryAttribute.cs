using System;

namespace Prise.Plugin
{
    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class PluginFactoryAttribute : System.Attribute { }
}