using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Prise.Core;
using Prise.Utils;

namespace Prise.AssemblyScanning
{
    public class DefaultNugetPackageAssemblyScanner : DefaultAssemblyScanner, IAssemblyScanner
    {
        private const string ExtractedDirectoryName = "_extracted";

        protected readonly INugetPackageUtilities nugetPackageUtilities;

        public DefaultNugetPackageAssemblyScanner(
            Func<string, IMetadataLoadContext> metadataLoadContextFactory,
            Func<IDirectoryTraverser> directoryTraverser,
            Func<INugetPackageUtilities> nugetPackageUtilities)
            : base(metadataLoadContextFactory, directoryTraverser)
        { this.nugetPackageUtilities = nugetPackageUtilities.ThrowIfNull(nameof(nugetPackageUtilities))(); }

        public override Task<IEnumerable<AssemblyScanResult>> Scan(IAssemblyScannerOptions options)
        {
            if (options == null)
                throw new ArgumentNullException($"{typeof(IAssemblyScannerOptions).Name} {nameof(options)}");

            var startingPath = options.StartingPath ?? throw new ArgumentException($"{nameof(options.StartingPath)}");
            var typeToScan = options.PluginType ?? throw new ArgumentException($"{nameof(options.PluginType)}");
            var fileTypes = options.FileTypes;

            if (!Path.IsPathRooted(startingPath))
                throw new AssemblyScanningException($"startingPath {startingPath} is not rooted, this must be a absolute path!");

            var packageFiles = this.nugetPackageUtilities.FindAllNugetPackagesFiles(startingPath);

            if (!packageFiles.Any())
                throw new AssemblyScanningException($"Scanning for NuGet packages had no results for Plugin Type {typeToScan.Name}");

            var packages = new List<PluginNugetPackage>();

            foreach (var packageFile in packageFiles)
            {
                var version = this.nugetPackageUtilities.GetVersionFromPackageFile(packageFile);
                var packageName = this.nugetPackageUtilities.GetPackageName(packageFile);
                packages.Add(new PluginNugetPackage
                {
                    Version = version,
                    FullPath = packageFile,
                    PackageName = packageName
                });
            }

            // Only take the latest of each package version
            foreach (var package in packages.GroupBy(p => p.PackageName, (key, g) => g.OrderByDescending(p => p.Version).First()))
            {
                var latestVersion = package.Version;
                var extractedNugetDirectory = Path.Combine(startingPath, ExtractedDirectoryName, package.PackageName);
                var hasMultipleVersions = packages.Count(p => p.PackageName == package.PackageName) > 1;
                var hasAlreadyBeenExtracted = this.nugetPackageUtilities.HasAlreadyBeenExtracted(extractedNugetDirectory);

                if (hasAlreadyBeenExtracted && !hasMultipleVersions)
                    continue;

                if (hasAlreadyBeenExtracted) // At this point, there are multiple versions of the same nuget package present in the directory
                {
                    var currentVersion = this.nugetPackageUtilities.GetCurrentVersionFromExtractedNuget(extractedNugetDirectory);
                    var hasNewerVersion = latestVersion > currentVersion;
                    if (!hasNewerVersion)
                        continue; // The latest version has already been extracted, nothing to do here

                    // Newer version was detected, delete the current version
                    this.nugetPackageUtilities.DeleteNugetDirectory(extractedNugetDirectory);
                }

                this.nugetPackageUtilities.UnCompressNugetPackage(package.FullPath, extractedNugetDirectory);
            }

            // Continue with default assembly scanning
            return base.Scan(new AssemblyScannerOptions
            {
                StartingPath = Path.Combine(startingPath, ExtractedDirectoryName),
                PluginType = typeToScan,
                FileTypes = fileTypes
            });
        }
    }
}