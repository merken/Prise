using System;

namespace Prise.Plugin
{
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class BootstrapperServiceAttribute : System.Attribute
    {
        Type serviceType;
        public Type ServiceType
        {
            get { return this.serviceType; }
            set { this.serviceType = value; }
        }

        Type bridgeType;
        public Type BridgeType
        {
            get { return this.bridgeType; }
            set { this.bridgeType = value; }
        }
    }
}