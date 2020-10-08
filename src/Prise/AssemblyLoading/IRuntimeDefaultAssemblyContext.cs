using System.Reflection;

namespace Prise.AssemblyLoading
{
    public interface IRuntimeDefaultAssemblyContext
    {
        Assembly LoadFromDefaultContext(AssemblyName assemblyName);
    }
}