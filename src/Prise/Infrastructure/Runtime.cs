using System.Diagnostics;

namespace Prise.Infrastructure
{
    [DebuggerDisplay("{RuntimeType.ToString()} - {Version} - {Location}")]
    public class Runtime
    {
        public RuntimeType RuntimeType { get; set; }
        public string Version { get; set; }
        public string Location { get; set; }
    }
}
