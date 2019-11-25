using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Prise.AssemblyScanning
{
    public interface IAssemblyScanner<T>
    {
        Task<IEnumerable<AssemblyScanResult<T>>> Scan();
    }
}
