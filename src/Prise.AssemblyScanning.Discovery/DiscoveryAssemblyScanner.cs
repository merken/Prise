using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise.AssemblyScanning.Discovery
{
    public class DiscoveryAssemblyScanner<T> : IAssemblyScanner<T>
    {
        protected readonly IAssemblyScannerOptions<T> options;
        protected bool disposed = false;
        // Use a list of IDisposables so that we can clean up later
        private IList<MetadataLoadContext> metadataLoadContexts;

        public DiscoveryAssemblyScanner(IAssemblyScannerOptions<T> options)
        {
            this.options = options;
            this.metadataLoadContexts = new List<MetadataLoadContext>();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                if (this.metadataLoadContexts != null && this.metadataLoadContexts.Any())
                    foreach (var context in this.metadataLoadContexts)
                        context.Dispose();
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

        private IEnumerable<string> ExcludeRuntimesFolder(IEnumerable<string> files) => files.Where(f => !f.Contains($"{Path.DirectorySeparatorChar}runtimes{Path.DirectorySeparatorChar}"));

        private IEnumerable<AssemblyScanResult<T>> DiscoverAssemblies(string startingPath, IEnumerable<string> searchPatterns, string typeToFind, string namespaceToFind)
        {
            var results = new List<AssemblyScanResult<T>>();
            foreach (var directoryPath in Directory.GetDirectories(startingPath))
            {
                var files = searchPatterns.SelectMany(p => Directory.GetFiles(directoryPath, p, SearchOption.AllDirectories));
                foreach (var assemblyFilePath in ExcludeRuntimesFolder(files))
                {
                    var implementation = GetImplementationsOfTypeFromAssembly(assemblyFilePath).FirstOrDefault();
                    if (implementation != null)
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
    }
}
