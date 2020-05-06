using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Prise.AssemblyScanning.Discovery.Nuget
{
    public class NugetPackageAssemblyScanner<T> : DiscoveryAssemblyScanner<T>, IAssemblyScanner<T>
    {
        private const string NugetExtension = "nupkg";
        private const string NuspecExtension = "nuspec";
        private const string ExtractedDirectoryName = "_extracted";

        public NugetPackageAssemblyScanner(IAssemblyScannerOptions<T> options) : base(options) { }

        public async Task<IEnumerable<AssemblyScanResult<T>>> Scan()
        {
            var startingPath = this.options.PathToScan;
            var searchPattern = $"*.{NugetExtension}";
            var packageFiles = Directory.GetFiles(startingPath, searchPattern, SearchOption.AllDirectories);
            var packages = new List<NugetPackage>();

            foreach (var packageFile in packageFiles)
            {
                var matches = Regex.Match(packageFile, @"^(.*?)\.((?:\.?[0-9]+){3,}(?:[-a-z]+)?)\.nupkg$").Groups;
                var versionString = matches[matches.Count - 1].Value;
                var version = new Version(versionString);
                var packageFileName = Path.GetFileName(packageFile);
                var packageNameWithoutVersion = packageFileName.Replace($".{versionString}", String.Empty);
                var packageNameWithoutExtension = packageNameWithoutVersion.Split($".{NugetExtension}")[0];

                packages.Add(new NugetPackage
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

                ZipFile.ExtractToDirectory(package.FullPath, extractionDirectory, true);
            }

            // Continue with assembly scanning
            return await base.Scan();
        }
    }
}
