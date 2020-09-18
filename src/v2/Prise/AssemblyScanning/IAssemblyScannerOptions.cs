using System;
using System.Collections.Generic;

namespace Prise.AssemblyScanning
{
    public interface IAssemblyScannerOptions
    {
        string StartingPath { get; }
        Type PluginType { get; }
        IEnumerable<string> FileTypes { get; }
    }
}