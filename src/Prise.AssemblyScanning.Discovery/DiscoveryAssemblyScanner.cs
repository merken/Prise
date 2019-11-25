using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace Prise.AssemblyScanning.Discovery
{
    public class DiscoveryAssemblyScanner<T> : IAssemblyScanner<T>
    {
        private readonly IAssemblyScannerOptions<T> options;

        public DiscoveryAssemblyScanner(IAssemblyScannerOptions<T> options)
        {
            this.options = options;
        }

        public Task<IEnumerable<AssemblyScanResult<T>>> Scan()
        {
            var typeToFind = typeof(T).Name;
            var namespaceToFind = typeof(T).Namespace;
            var results = new List<AssemblyScanResult<T>>();
            var startingPath = this.options.PathToScan;
            var searchPatterns = this.options.FileTypesToScan;

            var assemblies = new List<DiscoveredAssembly>();
            foreach (var directoryPath in Directory.GetDirectories(startingPath))
            {
                var files = searchPatterns.SelectMany(p => Directory.GetFiles(directoryPath, p, SearchOption.AllDirectories));
                foreach (var assemblyFilePath in files)
                {
                    assemblies.Add(DiscoverAssembly(assemblyFilePath));
                }
            }

            DiscoveredType foundType = null;
            var resolvedTypes = new List<DiscoveredType>();
            foreach (var assembly in assemblies)
            {
                assembly.Resolve(assemblies);
                resolvedTypes.AddRange(assembly.DefinedTypes);

                var type = assembly.DefinedTypes.FirstOrDefault(t => t.Name == typeToFind && t.Namespace == namespaceToFind);
                if (type != null && foundType == null)
                    foundType = type;
            }

            if (foundType == null)
                throw new AssemblyScanningException($"{typeToFind} was not found using DiscoveryAssemblyScanner");

            foreach (var assembly in assemblies)
            {
                if (assembly.ResolvedTypes.Any(t => t.Interfaces != null && t.Interfaces.Any(i => i?.Name == foundType.Name)))
                {
                    results.Add(new AssemblyScanResult<T>
                    {
                        AssemblyName = Path.GetFileName(assembly.Path),
                        AssemblyPath = Path.GetDirectoryName(assembly.Path)
                    });
                }
            }

            return Task.FromResult(results.AsEnumerable());
        }

        private unsafe DiscoveredAssembly DiscoverAssembly(string assemblyPath)
        {
            var fileInfo = new FileInfo(assemblyPath);
            long length = fileInfo.Length;
            var mapName = fileInfo.Name;
            var mode = FileMode.Open;
            var access = MemoryMappedFileAccess.Read;
            using (var file = MemoryMappedFile.CreateFromFile(assemblyPath, mode, mapName, length, access))
            {
                using (var stream = file.CreateViewStream(0x0, length, access))
                {
                    var headers = new PEHeaders(stream);
                    var start = (byte*)0;
                    stream.SafeMemoryMappedViewHandle.AcquirePointer(ref start);
                    var size = headers.MetadataSize;
                    var reader = new MetadataReader(start + headers.MetadataStartOffset, size, default(MetadataReaderOptions), null);
                    return new DiscoveredAssembly(assemblyPath, reader);
                }
            }
        }
    }
}
