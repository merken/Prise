using System.Reflection;

namespace Prise.AssemblyLoading
{
    public interface IRuntimeDefaultAssemblyContext
    {
        RuntimeAssemblyShim LoadFromDefaultContext(AssemblyName assemblyName);
    }
}