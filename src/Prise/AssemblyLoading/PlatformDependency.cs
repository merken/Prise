using NuGet.Versioning;

namespace Prise.AssemblyLoading
{
    public class PlatformDependency
    {
        public string DependencyNameWithoutExtension { get; set; }
        public SemanticVersion SemVer { get; set; }
        public string DependencyPath { get; set; }
        public string ProbingPath { get; set; }
    }
}