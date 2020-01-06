using System;
using System.Collections.Generic;
using Prise.AssemblyScanning;
using Prise.Infrastructure;

namespace Prise
{
    public class DefaultAssemblySelector<T> : IAssemblySelector<T>
    {
        private readonly Func<IEnumerable<AssemblyScanResult<T>>, IEnumerable<AssemblyScanResult<T>>> assemblySelector;

        public DefaultAssemblySelector(Func<IEnumerable<AssemblyScanResult<T>>, IEnumerable<AssemblyScanResult<T>>> assemblySelector = null)
        {
            this.assemblySelector = assemblySelector;
        }

        public IEnumerable<AssemblyScanResult<T>> SelectAssemblies(IEnumerable<AssemblyScanResult<T>> scanResults)
        {
            if (assemblySelector == null)
                return scanResults;
            return assemblySelector.Invoke(scanResults);
        }
    }
}
