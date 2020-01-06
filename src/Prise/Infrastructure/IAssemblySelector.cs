using Prise.AssemblyScanning;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Prise.Infrastructure
{
    /// <summary>
    /// An AssemblySelector selects the assemblies to load based on the types that were found after scanning the assembly
    /// </summary>
    public interface IAssemblySelector<T>
    {
        Task<IEnumerable<AssemblyScanResult<T>>> SelectAssemblies(IEnumerable<AssemblyScanResult<T>>);
    }
}
