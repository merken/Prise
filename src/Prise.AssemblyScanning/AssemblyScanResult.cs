using System;

namespace Prise.AssemblyScanning
{
    public class AssemblyScanResult<T>
    {
        public AssemblyScanResult()
        {
            this.PluginType = typeof(T);
        }

        public Type PluginType { get; private set; }
        public Type ImplementationType { get; set; }
        public string AssemblyPath { get; set; }
        public string AssemblyName { get; set; }
    }
}
