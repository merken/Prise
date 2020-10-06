using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Prise.Core;
using Prise.Utils;

namespace Prise.AssemblyScanning
{
    public class DefaultAssemblyScanner : IAssemblyScanner
    {
        protected IList<IDisposable> disposables;
        protected Func<string, IMetadataLoadContext> metadataLoadContextFactory;

        public DefaultAssemblyScanner(Func<string, IMetadataLoadContext> metadataLoadContextFactory)
        {
            this.disposables = new List<IDisposable>();
            this.metadataLoadContextFactory = metadataLoadContextFactory.ThrowIfNull(nameof(metadataLoadContextFactory));
        }

        public virtual Task<IEnumerable<AssemblyScanResult>> Scan(IAssemblyScannerOptions options)
        {
            if (options == null)
                throw new ArgumentNullException($"{typeof(IAssemblyScannerOptions).Name} {nameof(options)}");

            var startingPath = options.StartingPath ?? throw new ArgumentException($"{nameof(options.StartingPath)}");
            var typeToScan = options.PluginType ?? throw new ArgumentException($"{nameof(options.PluginType)}");
            var fileTypes = options.FileTypes;

            if (!Path.IsPathRooted(startingPath))
                throw new AssemblyScanningException($"startingPath {startingPath} is not rooted, this must be a absolute path!");

            if (fileTypes == null)
                fileTypes = new List<string> { "*.dll" };

            var results = new List<AssemblyScanResult>();
            foreach (var directoryPath in GetDirectoriesIncludingRoot(startingPath))
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

        private IEnumerable<string> GetDirectoriesIncludingRoot(string startingPath)
        {
            var directories = Directory.GetDirectories(startingPath);
            if (!directories.Any())
                directories = directories.Union(new[] { startingPath }).ToArray();

            return directories;
        }

        private IEnumerable<string> ExcludeRuntimesFolder(IEnumerable<string> files) => files.Where(f => !f.Contains($"{Path.DirectorySeparatorChar}runtimes{Path.DirectorySeparatorChar}"));

        private IEnumerable<Type> GetImplementationsOfTypeFromAssembly(Type type, string assemblyFullPath)
        {
            var context = this.metadataLoadContextFactory(assemblyFullPath);
            var assembly = context.LoadFromAssemblyName(Path.GetFileNameWithoutExtension(assemblyFullPath));
            this.disposables.Add(context);

            return assembly.Types
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
                if (this.disposables != null && this.disposables.Any())
                    foreach (var disposable in this.disposables)
                        disposable.Dispose();
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