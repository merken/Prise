using System.Reflection;
using System.Runtime.Loader;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Prise.AssemblyLoading
{
    public class RuntimeDefaultAssemblyContext : IRuntimeDefaultAssemblyContext
    {
        public RuntimeAssemblyShim LoadFromDefaultContext(AssemblyName assemblyName)
        {
            var candidate = GetLoadedAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
            var candidateName = candidate != null ? candidate.GetName() : null;

            if (candidateName != null && candidateName.Version != assemblyName.Version)
            {
                Debug.WriteLine($"Requested version for {assemblyName.Name} {assemblyName.Version} is not loaded in the app domain, loading version {candidateName.Version}");
                // Inject the dependency context here and load the correct version
                return new RuntimeAssemblyShim(AssemblyLoadContext.Default.LoadFromAssemblyName(candidateName), RuntimeLoadFlag.FromRuntimeVersion);
            }

            return new RuntimeAssemblyShim(AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName), RuntimeLoadFlag.FromRequestedVersion);
        }

#if SUPPORTS_LOADED_ASSEMBLIES
        protected IEnumerable<Assembly> GetLoadedAssemblies() => AssemblyLoadContext.Default.Assemblies;
#else
        protected IEnumerable<Assembly> GetLoadedAssemblies() => AppDomain.CurrentDomain.GetAssemblies();
#endif
    }
}