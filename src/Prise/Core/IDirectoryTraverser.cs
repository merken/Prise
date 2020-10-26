using System.Collections.Generic;

namespace Prise
{
    public interface IDirectoryTraverser
    {
        IEnumerable<string> TraverseDirectories(string startingPath);
        IEnumerable<string> TraverseFiles(string directory, IEnumerable<string> fileTypes);
    }
}