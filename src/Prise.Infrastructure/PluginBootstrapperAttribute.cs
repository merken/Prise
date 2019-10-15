using System;

namespace Prise.Infrastructure
{
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class PluginBootstrapperAttribute : System.Attribute
    {
        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        Type pluginType;
        public Type PluginType
        {
            get { return this.pluginType; }
            set { this.pluginType = value; }
        }
    }
   
}