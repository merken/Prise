using System.Diagnostics;
using System.Reflection;
using System.Runtime.Versioning;

namespace Prise.Infrastructure
{
    public interface IHostFrameworkProvider
    {
        string ProvideHostFramwork();
    }

    [DebuggerDisplay("{GetAssemblyName()}")]
    public class HostFrameworkProvider : IHostFrameworkProvider
    {
        public virtual string ProvideHostFramwork() => Assembly
            .GetEntryAssembly()?
            .GetCustomAttribute<TargetFrameworkAttribute>()?
            .FrameworkName;
    }
}
