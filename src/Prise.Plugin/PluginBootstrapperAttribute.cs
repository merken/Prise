using System;

namespace Prise.Plugin
{
    [Obsolete("Usage of a PluginBootstrapper is obsolete in favor of field injection using the " + nameof(PluginServiceAttribute) + ". Existing plugins will continue to function as normal.", false)]
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