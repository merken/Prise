using System.Diagnostics;
using System.Reflection;

namespace Prise.Infrastructure
{
    [DebuggerDisplay("{DependencyName.Name}")]
    public class HostDependency
    {
        public AssemblyName DependencyName { get; set; }
    }
}
