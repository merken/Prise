using System.Reflection;

namespace Prise.AssemblyLoading
{
    public class HostDependency
    {
        public AssemblyName DependencyName { get; set; }
        public bool AllowDowngrade { get; set; }
    }
}