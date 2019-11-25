using System.Collections.Generic;

namespace Prise.AssemblyScanning
{
    public interface IAssemblyScannerOptions<T>
    {
        string PathToScan { get; }
        IEnumerable<string> FileTypesToScan { get; }
    }
}
