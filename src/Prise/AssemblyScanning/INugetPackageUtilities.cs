using System;
using System.Collections.Generic;

namespace Prise.AssemblyScanning
{
    public interface INugetPackageUtilities
    {
        IEnumerable<string> FindAllNugetPackagesFiles(string startingPath);
        Version GetVersionFromPackageFile(string packageFile);
        string GetPackageName(string packageFile);
        void UnCompressNugetPackage(string packageFile, string extractedNugetDirectory);
        bool HasAlreadyBeenExtracted(string extractedNugetDirectory);
        Version GetCurrentVersionFromExtractedNuget(string extractedNugetDirectory);
        void DeleteNugetDirectory(string extractedNugetDirectory);
    }
}