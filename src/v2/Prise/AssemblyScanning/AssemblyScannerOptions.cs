using System;
using System.Collections.Generic;

namespace Prise.AssemblyScanning
{
    public class AssemblyScannerOptions : IAssemblyScannerOptions
    {
        public string StartingPath { get; set; }

        public Type PluginType { get; set; }

        public IEnumerable<string> FileTypes { get; set; }
    }
}