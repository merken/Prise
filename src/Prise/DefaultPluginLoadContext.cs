using Prise.AssemblyScanning;
using Prise.Infrastructure;

namespace Prise
{
    public class DefaultPluginLoadContext<T> : IPluginLoadContext
    {
        public string PluginAssemblyName { get; private set; }

        public string PluginAssemblyPath { get; private set; }

        public DefaultPluginLoadContext(string assemblyName, string assemblyPath)
        {
            this.PluginAssemblyName = assemblyName;
            this.PluginAssemblyPath = assemblyPath;
        }

        public static IPluginLoadContext FromAssemblyScanResult(AssemblyScanResult<T> assemblyScanResult)
            => new DefaultPluginLoadContext<T>(assemblyScanResult.AssemblyName, assemblyScanResult.AssemblyPath);
    }
}
