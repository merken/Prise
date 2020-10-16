using System.Reflection;
using System.Runtime.Versioning;

namespace Prise.IntegrationTestsHost
{
    public interface IHostFrameworkProvider
    {
        string ProvideHostFramework();
    }
    
    /// <summary>
    /// This is required for testing
    /// </summary>
    public class AppHostFrameworkProvider : IHostFrameworkProvider
    {
        public string ProvideHostFramework() => typeof(AppHostFrameworkProvider).Assembly
            .GetCustomAttribute<TargetFrameworkAttribute>()?
            .FrameworkName;
    }
}