using System.Diagnostics;
using System.Reflection;

namespace Prise.Infrastructure
{
    [DebuggerDisplay("{DependencyName.Name}")]
    public class RemoteDependency
    {
        public AssemblyName DependencyName { get; set; }
    }
}
