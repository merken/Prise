using System;

namespace Prise.Plugin
{
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PluginAttribute : System.Attribute
    {
        Type pluginType;
        public Type PluginType
        {
            get { return this.pluginType; }
            set { this.pluginType = value; }
        }
    }
}