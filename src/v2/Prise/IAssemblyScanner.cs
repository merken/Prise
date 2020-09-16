using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Prise
{
    internal class PluginNugetPackage
    {
        public Version Version { get; set; }
        public string FullPath { get; set; }
        public string PackageName { get; set; }
    }

    [Serializable]
    public class AssemblyScanningException : Exception
    {
        public AssemblyScanningException(string message) : base(message)
        {
        }

        public AssemblyScanningException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public AssemblyScanningException()
        {
        }

        protected AssemblyScanningException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class AssemblyScanResult
    {
        public Type ContractType { get; set; }

        /// <summary>
        /// The PluginType will be null when the platform is older than netcoreapp3.0
        /// </summary>
        public Type PluginType { get; set; }
        public string PluginTypeName { get; set; }
        public string PluginTypeNamespace { get; set; }
        public string AssemblyPath { get; set; }
        public string AssemblyName { get; set; }
    }

    public interface IAssemblyScanner
    {
        Task<IEnumerable<AssemblyScanResult>> Scan(string startingPath, Type type, IEnumerable<string> fileTypes = null);
    }

    public class DefaultAssemblyResolver : MetadataAssemblyResolver
    {
        private readonly string assemblyPath;
        private readonly string[] platformAssemblies = new[] { "mscorlib", "netstandard", "System.Private.CoreLib", "System.Runtime" };

        public DefaultAssemblyResolver(string fullPathToAssembly)
        {
            this.assemblyPath = Path.GetDirectoryName(fullPathToAssembly);
        }

        public override Assembly Resolve(MetadataLoadContext context, AssemblyName assemblyName)
        {
            // We know these assemblies are located in the Host, so don't bother loading them from the plugin location
            if (this.platformAssemblies.Contains(assemblyName.Name))
                return context.LoadFromAssemblyPath(AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName).Location);

            // Check if the file is found in the plugin location
            var candidateFile = Path.Combine(assemblyPath, $"{assemblyName.Name}.dll");
            if (File.Exists(candidateFile))
                return context.LoadFromAssemblyPath(candidateFile);

            // Fallback, load from Host AppDomain, this is mostly required for System.* assemblies
            return context.LoadFromAssemblyPath(AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName).Location);
        }
    }

    public class DefaultAssemblyScanner : IAssemblyScanner, IDisposable
    {
        protected bool disposed = false;
        private IList<MetadataLoadContext> metadataLoadContexts;

        public DefaultAssemblyScanner()
        {
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

        public virtual Task<IEnumerable<AssemblyScanResult>> Scan(string startingPath, Type type, IEnumerable<string> fileTypes = null)
        {
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
                    var implementation = GetImplementationsOfTypeFromAssembly(type, assemblyFilePath).FirstOrDefault();
                    if (implementation != null)
                        results.Add(new AssemblyScanResult
                        {
                            ContractType = type,
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
    }

    public class NugetPackageAssemblyScanner : DefaultAssemblyScanner, IAssemblyScanner, IDisposable
    {
        private const string NugetExtension = "nupkg";
        private const string NuspecExtension = "nuspec";
        private const string ExtractedDirectoryName = "_extracted";

        public override Task<IEnumerable<AssemblyScanResult>> Scan(string startingPath, Type type, IEnumerable<string> fileTypes = null)
        {
            var searchPattern = $"*.{NugetExtension}";
            var packageFiles = Directory.GetFiles(startingPath, searchPattern, SearchOption.AllDirectories);

            if (!packageFiles.Any())
                throw new AssemblyScanningException($"Scanning for NuGet packages had no results for Plugin Type {type.Name}");

            var packages = new List<PluginNugetPackage>();

            foreach (var packageFile in packageFiles)
            {
                var matches = Regex.Match(packageFile, @"^(.*?)\.((?:\.?[0-9]+){3,}(?:[-a-z]+)?)\.nupkg$").Groups;
                var versionString = matches[matches.Count - 1].Value;
                var version = new Version(versionString);
                var packageFileName = Path.GetFileName(packageFile);
                var packageNameWithoutVersion = packageFileName.Replace($".{versionString}", String.Empty);
                var packageNameWithoutExtension = packageNameWithoutVersion.Split(new[] { $".{NugetExtension}" }, StringSplitOptions.RemoveEmptyEntries)[0];

                packages.Add(new PluginNugetPackage
                {
                    Version = version,
                    FullPath = packageFile,
                    PackageName = packageNameWithoutExtension
                });
            }

            // Only take the latest of each package version
            foreach (var package in packages.GroupBy(p => p.PackageName, (key, g) => g.OrderByDescending(p => p.Version).First()))
            {
                var latestVersion = package.Version;
                var extractionDirectory = Path.Combine(startingPath, ExtractedDirectoryName, package.PackageName);
                var hasMultipleVersions = packages.Count(p => p.PackageName == package.PackageName) > 1;
                var hasAlreadyBeenExtracted = Directory.Exists(extractionDirectory);

                if (hasAlreadyBeenExtracted && !hasMultipleVersions)
                    continue;

                if (hasAlreadyBeenExtracted)
                {
                    var currentNuspec = Directory.GetFiles(extractionDirectory, $"*.{NuspecExtension}", SearchOption.AllDirectories).FirstOrDefault();
                    var currentVersionAsString = XDocument.Load(currentNuspec).Root
                                        .DescendantNodes().OfType<XElement>()
                                        .FirstOrDefault(x => x.Name.LocalName.Equals("version"))?.Value;

                    var currentVersion = !String.IsNullOrEmpty(currentVersionAsString) ? new Version(currentVersionAsString) : new Version("0.0.0");
                    var hasNewerVersion = latestVersion > currentVersion;
                    if (!hasNewerVersion)
                        continue;

                    // Newer version was detected, delete the current version
                    Directory.Delete(extractionDirectory, true);
                }

                ZipFile.ExtractToDirectory(package.FullPath, extractionDirectory);
            }

            // Continue with assembly scanning
            return base.Scan(Path.Combine(startingPath, ExtractedDirectoryName), type);
        }
    }
}