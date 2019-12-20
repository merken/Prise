using System.Collections.Generic;
using System.Diagnostics;

namespace Prise.Infrastructure
{
    public class RuntimeInformation
    {
        [DebuggerDisplay("{Runtimes?.Count}")]
        public IEnumerable<Runtime> Runtimes { get; set; }
    }
}
