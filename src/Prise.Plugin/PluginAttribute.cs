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

    public enum ProvidedBy
    {
        Plugin = 0,
        Host
    }

    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class PluginServiceAttribute : System.Attribute
    {
        Type serviceType;
        public Type ServiceType
        {
            get { return this.serviceType; }
            set { this.serviceType = value; }
        }

        ProvidedBy providedBy;
        public ProvidedBy ProvidedBy
        {
            get { return this.providedBy; }
            set { this.providedBy = value; }
        }

        Type bridgeType;
        public Type BridgeType
        {
            get { return this.bridgeType; }
            set { this.bridgeType = value; }
        }
    }
}