using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Prise.V2
{
    public interface IAssemblyScanner
    {
        Task<IEnumerable<AssemblyScanResult>> Scan(string startingPath, Type type, IEnumerable<string> fileTypes = null);
    }
}