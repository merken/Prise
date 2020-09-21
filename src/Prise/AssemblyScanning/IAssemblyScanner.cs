using System.Collections.Generic;
using System.Threading.Tasks;
using Prise.Core;

namespace Prise.AssemblyScanning
{
    public interface IAssemblyScanner
    {
        Task<IEnumerable<AssemblyScanResult>> Scan(IAssemblyScannerOptions options);
    }
}