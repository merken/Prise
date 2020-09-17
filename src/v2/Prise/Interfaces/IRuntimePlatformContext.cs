using System.Collections.Generic;

namespace Prise.V2
{
    public interface IRuntimePlatformContext
    {
        IEnumerable<string> GetPlatformExtensions();
        IEnumerable<string> GetPluginDependencyNames(string nameWithoutFileExtension);
        IEnumerable<string> GetPlatformDependencyNames(string nameWithoutFileExtension);
        RuntimeInfo GetRuntimeInfo();
    }
}