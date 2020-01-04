using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
#if NETCORE2_1
using System.IO.MemoryMappedFiles;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
#endif
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
            var startingPath = this.options.PathToScan;
            var searchPatterns = this.options.FileTypesToScan;

            return Task.FromResult(DiscoverAssemblies(startingPath, searchPatterns, typeToFind, namespaceToFind));
        }

#if NETCORE2_1
        private IEnumerable<AssemblyScanResult<T>> DiscoverAssemblies(string startingPath, IEnumerable<string> searchPatterns, string typeToFind, string namespaceToFind)
        {
            var results = new List<AssemblyScanResult<T>>();
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
            return results;
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
#endif

#if NETCORE3_0
        private IEnumerable<AssemblyScanResult<T>> DiscoverAssemblies(string startingPath, IEnumerable<string> searchPatterns, string typeToFind, string namespaceToFind)
        {
            var results = new List<AssemblyScanResult<T>>();
            foreach (var directoryPath in Directory.GetDirectories(startingPath))
            {
                var files = searchPatterns.SelectMany(p => Directory.GetFiles(directoryPath, p, SearchOption.AllDirectories));
                foreach (var assemblyFilePath in files)
                {
                    if (DoesAssemblyContainImplementationsOfType(typeToFind, namespaceToFind, assemblyFilePath))
                        results.Add(new AssemblyScanResult<T>
                        {
                            AssemblyName = Path.GetFileName(assemblyFilePath),
                            AssemblyPath = Path.GetDirectoryName(assemblyFilePath)
                        });
                }
            }
            return results;
        }

        private bool DoesAssemblyContainImplementationsOfType(string type, string @namespace, string assemblyFullPath)
        {
            var resolver = new DefaultAssemblyResolver(assemblyFullPath);
            using (var loadContext = new MetadataLoadContext(resolver))
            {
                var assembly = loadContext.LoadFromAssemblyName(Path.GetFileNameWithoutExtension(assemblyFullPath));
                return assembly.GetTypes().Any(t => t.GetInterfaces().Any(i => i.Name == type && i.Namespace == @namespace));
            }
        }
#endif
    }
}
