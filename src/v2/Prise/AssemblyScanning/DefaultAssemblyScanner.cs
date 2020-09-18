using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Prise.Core;

namespace Prise.AssemblyScanning
{
    public class DefaultAssemblyScanner : IAssemblyScanner, IDisposable
    {
        private IList<MetadataLoadContext> metadataLoadContexts;

        public DefaultAssemblyScanner()
        {
            this.metadataLoadContexts = new List<MetadataLoadContext>();
        }        

        public virtual Task<IEnumerable<AssemblyScanResult>> Scan(IAssemblyScannerOptions options)
        {
            var startingPath = options.StartingPath;
            var typeToScan = options.PluginType;
            var fileTypes = options.FileTypes;

            if (!Path.IsPathRooted(startingPath))
                throw new AssemblyScanningException($"startingPath {startingPath} is not rooted, this must be a absolute path!");

            if (fileTypes == null)
                fileTypes = new List<string> { "*.dll" };

            var results = new List<AssemblyScanResult>();
            foreach (var directoryPath in Directory.GetDirectories(startingPath))
            {
                var files = fileTypes.SelectMany(p => Directory.GetFiles(directoryPath, p, SearchOption.AllDirectories));
                foreach (var assemblyFilePath in ExcludeRuntimesFolder(files))
                {
                    var implementation = GetImplementationsOfTypeFromAssembly(typeToScan, assemblyFilePath).FirstOrDefault();
                    if (implementation != null)
                        results.Add(new AssemblyScanResult
                        {
                            ContractType = typeToScan,
                            AssemblyName = Path.GetFileName(assemblyFilePath),
                            AssemblyPath = Path.GetDirectoryName(assemblyFilePath),
                            PluginType = implementation
                        });
                }
            }
            
            return Task.FromResult(results.AsEnumerable());
        }

        private IEnumerable<string> ExcludeRuntimesFolder(IEnumerable<string> files) => files.Where(f => !f.Contains($"{Path.DirectorySeparatorChar}runtimes{Path.DirectorySeparatorChar}"));

        private IEnumerable<Type> GetImplementationsOfTypeFromAssembly(Type type, string assemblyFullPath)
        {
            var context = new MetadataLoadContext(new DefaultAssemblyResolver(assemblyFullPath));
            var assembly = context.LoadFromAssemblyName(Path.GetFileNameWithoutExtension(assemblyFullPath));
            this.metadataLoadContexts.Add(context);

            return assembly.GetTypes()
                        .Where(t => t.CustomAttributes
                            .Any(c => c.AttributeType.Name == typeof(Prise.Plugin.PluginAttribute).Name
                            && (c.NamedArguments.First(a => a.MemberName == "PluginType").TypedValue.Value as Type).Name == type.Name
                            && (c.NamedArguments.First(a => a.MemberName == "PluginType").TypedValue.Value as Type).Namespace == type.Namespace))
                        .OrderBy(t => t.Name)
                        .ToList();
        }

        protected bool disposed = false;
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
    }
}