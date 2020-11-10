using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Prise.AssemblyScanning
{
    public class DefaultNugetPackageUtilities : INugetPackageUtilities
    {
        public IEnumerable<string> FindAllNugetPackagesFiles(string startingPath)
        {
            return Directory.GetFiles(startingPath, "*.nupkg", SearchOption.AllDirectories);
        }

        public Version GetVersionFromPackageFile(string packageFile)
        {
            return new Version(GetVersionStringForPackageFile(packageFile));
        }

        public string GetPackageName(string packageFile)
        {
            var packageFileName = Path.GetFileName(packageFile);
            var packageNameWithoutVersion = packageFileName.Replace($".{GetVersionStringForPackageFile(packageFile)}", String.Empty);
            return packageNameWithoutVersion.Split(new[] { $".nupkg" }, StringSplitOptions.RemoveEmptyEntries)[0];
        }

        public void UnCompressNugetPackage(string packageFile, string extractedNugetDirectory)
        {
            ZipFile.ExtractToDirectory(packageFile, extractedNugetDirectory);
        }

        public bool HasAlreadyBeenExtracted(string extractedNugetDirectory)
        {
            return Directory.Exists(extractedNugetDirectory);
        }

        private string GetVersionStringForPackageFile(string packageFile)
        {
            var matches = Regex.Match(packageFile, @"^(.*?)\.((?:\.?[0-9]+){3,}(?:[-a-z]+)?)\.nupkg$").Groups;
            return matches[matches.Count - 1].Value;
        }
    }
}