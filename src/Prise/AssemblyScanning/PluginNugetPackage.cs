using System;

namespace Prise.AssemblyScanning
{
    internal class PluginNugetPackage
    {
        public Version Version { get; set; }
        public string FullPath { get; set; }
        public string PackageName { get; set; }
    }
}