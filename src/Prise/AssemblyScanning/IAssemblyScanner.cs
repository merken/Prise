using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Prise.Core;

namespace Prise.AssemblyScanning
{
    public interface IAssemblyScanner : IDisposable
    {
        Task<IEnumerable<AssemblyScanResult>> Scan(IAssemblyScannerOptions options);
    }
}