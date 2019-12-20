using System.Diagnostics;

namespace Prise.Infrastructure
{

    [DebuggerDisplay("{DependencyNameWithoutExtension}")]
    public class PlatformDependency
    {
        public string DependencyNameWithoutExtension { get; set; }
        public string Version { get; set; }
        public string DependencyPath { get; set; }
        public string ProbingPath { get; set; }
    }
}
