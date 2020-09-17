using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Prise.AssemblyScanning
{
    public interface IAssemblyScanner
    {
        Task<IEnumerable<AssemblyScanResult>> Scan(string startingPath, Type type, IEnumerable<string> fileTypes = null);
    }
}