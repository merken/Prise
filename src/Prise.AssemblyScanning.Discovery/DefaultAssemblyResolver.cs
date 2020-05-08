using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Prise.AssemblyScanning.Discovery
{
    public class DefaultAssemblyResolver : MetadataAssemblyResolver
    {
        private readonly string assemblyPath;
        private readonly string[] platformAssemblies = new[] { "mscorlib", "netstandard", "System.Private.CoreLib", "System.Runtime" };

        public DefaultAssemblyResolver(string fullPathToAssembly)
        {
            this.assemblyPath = Path.GetDirectoryName(fullPathToAssembly);
        }

        public override Assembly Resolve(MetadataLoadContext context, AssemblyName assemblyName)
        {
            // We know these assemblies are located in the Host, so don't bother loading them from the plugin location
            if (this.platformAssemblies.Contains(assemblyName.Name))
                return context.LoadFromAssemblyPath(AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName).Location);

            // Check if the file is found in the plugin location
            var candidateFile = Path.Combine(assemblyPath, $"{assemblyName.Name}.dll");
            if (File.Exists(candidateFile))
                return context.LoadFromAssemblyPath(candidateFile);

            // Fallback, load from Host AppDomain, this is mostly required for System.* assemblies
            return context.LoadFromAssemblyPath(AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName).Location);
        }
    }
}