using System.Reflection;

namespace Prise.AssemblyLoading
{
    public class RuntimeDefaultAssemblyContext : IRuntimeDefaultAssemblyContext
    {
        public Assembly LoadFromDefaultContext(AssemblyName assemblyName)
        {
            return System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
        }
    }
}