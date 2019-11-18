using System.Collections.Generic;

namespace Prise.Infrastructure
{
    public interface IRuntimePlatformContext
    {
        IEnumerable<string> GetPlatformExtensions();
        IEnumerable<string> GetPluginDependencyNames(string nameWithoutFileExtension);
        IEnumerable<string> GetPlatformDependencyNames(string nameWithoutFileExtension);
        RuntimeInformation GetRuntimeInformation();
    }
}
