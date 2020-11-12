using System.Reflection;
using System.Runtime.Loader;
using System.Linq;

namespace Prise.AssemblyLoading
{
    public class RuntimeDefaultAssemblyContext : IRuntimeDefaultAssemblyContext
    {
        public Assembly LoadFromDefaultContext(AssemblyName assemblyName)
        {
            var candidate = AssemblyLoadContext.Default.Assemblies.FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
            var candidateName = candidate != null ? candidate.GetName() : null;

            if (candidateName != null && candidateName.Version != assemblyName.Version)
            {
                return System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyName(candidateName);
            }

            return AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
        }
    }
}