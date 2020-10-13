using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Prise.Core
{
    public class DefaultDirectoryTraverser : IDirectoryTraverser
    {
        public IEnumerable<string> TraverseDirectories(string startingPath)
        {
            var directories = Directory.GetDirectories(startingPath);
            if (!directories.Any())
                directories = directories.Union(new[] { startingPath }).ToArray();

            return directories;
        }

        public IEnumerable<string> TraverseFiles(string directory, IEnumerable<string> fileTypes)
        {
            return fileTypes
                .SelectMany(p => Directory.GetFiles(directory, p, SearchOption.AllDirectories))
                // ExcludeRuntimesFolder
                .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}runtimes{Path.DirectorySeparatorChar}"));
        }

        private IEnumerable<string> ExcludeRuntimesFolder(IEnumerable<string> files) => files.Where(f => !f.Contains($"{Path.DirectorySeparatorChar}runtimes{Path.DirectorySeparatorChar}"));
    }
}