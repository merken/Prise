using Prise.Infrastructure;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Versioning;

namespace Prise
{
    [DebuggerDisplay("{ProvideHostFramwork()}")]
    public class HostFrameworkProvider : IHostFrameworkProvider
    {
        public virtual string ProvideHostFramwork() => Assembly
            .GetEntryAssembly()?
            .GetCustomAttribute<TargetFrameworkAttribute>()?
            .FrameworkName;
    }
}
