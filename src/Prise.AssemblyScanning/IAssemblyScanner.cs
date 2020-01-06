using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Prise.AssemblyScanning
{
    public interface IAssemblyScanner<T> : IDisposable
    {
        Task<IEnumerable<AssemblyScanResult<T>>> Scan();
    }
}
