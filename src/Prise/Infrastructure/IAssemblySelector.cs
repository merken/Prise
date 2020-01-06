using System.Collections.Generic;
using Prise.AssemblyScanning;

namespace Prise.Infrastructure
{
    /// <summary>
    /// An AssemblySelector selects the assemblies to load based on the types that were found after scanning the assembly
    /// </summary>
    public interface IAssemblySelector<T>
    {
        IEnumerable<AssemblyScanResult<T>> SelectAssemblies(IEnumerable<AssemblyScanResult<T>> scanResults);
    }
}
