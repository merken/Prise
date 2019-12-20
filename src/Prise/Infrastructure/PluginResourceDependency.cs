using System.Diagnostics;

namespace Prise.Infrastructure
{
    [DebuggerDisplay("{Path}")]
    public class PluginResourceDependency
    {
        public string Path { get; set; }
    }
}
