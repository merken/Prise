using System;

namespace Prise.AssemblyScanning.Discovery.Nuget
{
    internal class NugetPackage
    {
        public Version Version { get; set; }
        public string FullPath { get; set; }
        public string PackageName { get; set; }
    }
}
