#if NETCORE3_0
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
        private readonly string[] platformAssemblies = new[] { "mscorlib", "netstandard" };

        public DefaultAssemblyResolver(string fullPathToAssembly)
        {
            this.assemblyPath = Path.GetDirectoryName(fullPathToAssembly);
        }

        public override Assembly Resolve(MetadataLoadContext context, AssemblyName assemblyName)
        {
            if (this.platformAssemblies.Contains(assemblyName.Name) || assemblyName.Name.StartsWith("System."))
                return context.LoadFromAssemblyPath(AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName).Location);

            return context.LoadFromAssemblyPath(Path.Combine(assemblyPath, $"{assemblyName.Name}.dll"));
        }
    }
}
#endif