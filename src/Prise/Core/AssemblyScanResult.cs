using System;

namespace Prise
{
    public class AssemblyScanResult
    {
        public Type ContractType { get; set; }

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