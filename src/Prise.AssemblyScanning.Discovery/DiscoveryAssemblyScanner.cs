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
        protected bool disposed = false;
#if NETCORE3_0
        // Use a list of IDisposables so that we can clean up later
        private IList<MetadataLoadContext> metadataLoadContexts;
#endif

        public DiscoveryAssemblyScanner(IAssemblyScannerOptions<T> options)
        {
            this.options = options;
#if NETCORE3_0
            this.metadataLoadContexts = new List<MetadataLoadContext>();
#endif
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
#if NETCORE3_0
                if (this.metadataLoadContexts != null && this.metadataLoadContexts.Any())
                    foreach (var context in this.metadataLoadContexts)
                        context.Dispose();
#endif
                GC.Collect(); // collects all unused memory
                GC.WaitForPendingFinalizers(); // wait until GC has finished its work
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
                var files = searchPatterns.SelectMany(p => ExcludeRuntimesFolder(Directory.GetFiles(directoryPath, p, SearchOption.AllDirectories)));
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
                // Iterate all found plugin types
                foreach (var type in assembly.ResolvedTypes.Where(t => t.Interfaces != null && t.Interfaces.Any(i => i?.Name == foundType.Name)))
                {
                    results.Add(new AssemblyScanResult<T>
                    {
                        AssemblyName = Path.GetFileName(assembly.Path),
                        AssemblyPath = Path.GetDirectoryName(assembly.Path),
                        PluginTypeName = type.Name,
                        PluginTypeNamespace = type.Namespace
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

        private IEnumerable<string> ExcludeRuntimesFolder(IEnumerable<string> files) => files.Where(f => !f.Contains($"{Path.DirectorySeparatorChar}runtimes{Path.DirectorySeparatorChar}"));

#if NETCORE3_0
        private IEnumerable<AssemblyScanResult<T>> DiscoverAssemblies(string startingPath, IEnumerable<string> searchPatterns, string typeToFind, string namespaceToFind)
        {
            var results = new List<AssemblyScanResult<T>>();
            foreach (var directoryPath in Directory.GetDirectories(startingPath))
            {
                var files = searchPatterns.SelectMany(p => Directory.GetFiles(directoryPath, p, SearchOption.AllDirectories));
                foreach (var assemblyFilePath in ExcludeRuntimesFolder(files))
                {
                    foreach (var implementation in GetImplementationsOfTypeFromAssembly(assemblyFilePath))
                        results.Add(new AssemblyScanResult<T>
                        {
                            AssemblyName = Path.GetFileName(assemblyFilePath),
                            AssemblyPath = Path.GetDirectoryName(assemblyFilePath),
                            PluginType = implementation
                        });
                }
            }
            return results;
        }

        private IEnumerable<Type> GetImplementationsOfTypeFromAssembly(string assemblyFullPath)
        {
            var context = new MetadataLoadContext(new DefaultAssemblyResolver(assemblyFullPath));
            var assembly = context.LoadFromAssemblyName(Path.GetFileNameWithoutExtension(assemblyFullPath));
            this.metadataLoadContexts.Add(context);

            return assembly.GetTypes()
                        .Where(t => t.CustomAttributes
                            .Any(c => c.AttributeType.Name == typeof(Prise.Plugin.PluginAttribute).Name
                            && (c.NamedArguments.First(a => a.MemberName == "PluginType").TypedValue.Value as Type).Name == typeof(T).Name
                            && (c.NamedArguments.First(a => a.MemberName == "PluginType").TypedValue.Value as Type).Namespace == typeof(T).Namespace))
                        .OrderBy(t => t.Name)
                        .ToList();
        }
#endif
    }
}
