using System;

namespace Prise.AssemblyScanning
{
    public class AssemblyScanResult<T>
    {
        public AssemblyScanResult()
        {
            this.ContractType = typeof(T);
        }

        public Type ContractType { get; private set; }

        /// <summary>
        /// The PluginType will be null when the platform is older than netcoreapp3.0
        /// </summary>
        public Type PluginType { get; set; }
        public string PluginTypeName { get; set; }
        public string PluginTypeNamespace { get; set; }
        public string AssemblyPath { get; set; }
        public string AssemblyName { get; set; }
    }
}
