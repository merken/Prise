using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Prise.Utils;

namespace Prise.AssemblyScanning
{
    /// <summary>
    /// This class is not tested
    /// </summary>
    public class DefaultAssemblyResolver : MetadataAssemblyResolver
    {
        private readonly string assemblyPath;
        private readonly string[] platformAssemblies = new[] { "mscorlib", "netstandard", "System.Private.CoreLib", "System.Runtime" };

        public DefaultAssemblyResolver(string fullPathToAssembly)
        {
            this.assemblyPath = Path.GetDirectoryName(fullPathToAssembly.ThrowIfNullOrEmpty(nameof(fullPathToAssembly)));
        }

        public override Assembly Resolve(MetadataLoadContext context, AssemblyName assemblyName)
        {
            // We know these assemblies are located in the Host, so don't bother loading them from the plugin location
            if (this.platformAssemblies.Contains(assemblyName.Name))
            {
                return LoadAssemblyForRuntime(context, assemblyName);
            }

            // Check if the file is found in the plugin location
            var candidateFile = Path.Combine(assemblyPath, $"{assemblyName.Name}.dll");
            if (File.Exists(candidateFile))
                return context.LoadFromAssemblyPath(candidateFile);

            // Fallback, load from Host AppDomain, this is mostly required for System.* assemblies
            return LoadAssemblyForRuntime(context, assemblyName);
        }

        protected Assembly LoadAssemblyForRuntime(MetadataLoadContext context, AssemblyName assemblyName)
        {
            try
            {
                var candidate = AssemblyLoadContext.Default.Assemblies.FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
                var candidateName = candidate != null ? candidate.GetName() : null;

                if (candidateName != null && candidateName.Version != assemblyName.Version)
                {
                    return context.LoadFromAssemblyPath(AssemblyLoadContext.Default.LoadFromAssemblyName(candidateName).Location);
                }

                return context.LoadFromAssemblyPath(AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName).Location);
            }
            catch (FileNotFoundException) when (assemblyName?.Name == "System.Runtime")
            {
                var hostRuntimeAssembly = typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly;
                throw new AssemblyScanningException($"System.Runtime {assemblyName.Version} failed to load. Are you trying to load a new plugin into an old host? Host Runtime Version: {hostRuntimeAssembly.GetName().Version} on {hostRuntimeAssembly.CodeBase}");
            }
        }
    }
}