using System;

namespace Prise.Plugin
{
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class PluginBootstrapperAttribute : System.Attribute
    {
        Type pluginType;
        public Type PluginType
        {
            get { return this.pluginType; }
            set { this.pluginType = value; }
        }
    }
}