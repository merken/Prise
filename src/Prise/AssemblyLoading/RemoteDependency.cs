using System.Reflection;
using NuGet.Versioning;

namespace Prise.AssemblyLoading
{
    public class RemoteDependency
    {
        public AssemblyName DependencyName { get; set; }
        public SemanticVersion SemVer => new SemanticVersion(DependencyName.Version.Major, DependencyName.Version.Minor, DependencyName.Version.Revision);
    }
}