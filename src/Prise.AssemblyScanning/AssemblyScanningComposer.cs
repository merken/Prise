using System;

namespace Prise.AssemblyScanning
{
    public class AssemblyScanningComposition<T>
    {
        public IAssemblyScanner<T> Scanner { get; internal set; }
        public IAssemblyScannerOptions<T> ScannerOptions { get; internal set; }
        public Type ScannerType { get; internal set; }
        public Type ScannerOptionsType { get; internal set; }
    }

    public class AssemblyScanningComposer<T>
    {
        private IAssemblyScanner<T> scanner;
        private Type scannerType;
        private IAssemblyScannerOptions<T> scannerOptions;
        private Type scannerOptionsType;

        public AssemblyScanningComposer() { }

        public AssemblyScanningComposition<T> Compose()
        {
            return new AssemblyScanningComposition<T>
            {
                Scanner = this.scanner,
                ScannerType = this.scannerType,
                ScannerOptions = this.scannerOptions,
                ScannerOptionsType = this.scannerOptionsType
            };
        }

        public AssemblyScanningComposer<T> WithDefaultOptions<TScanner, TScannerOptions>()
            where TScanner : IAssemblyScanner<T>
            where TScannerOptions : IAssemblyScannerOptions<T>
        {
            this.scannerType = typeof(TScanner);
            this.scannerOptionsType = typeof(TScannerOptions);
            return this;
        }

        public AssemblyScanningComposer<T> WithAssemblyScanner<TScanner>()
            where TScanner : IAssemblyScanner<T>
        {
            this.scannerType = typeof(TScanner);
            return this;
        }

        public AssemblyScanningComposer<T> WithAssemblyScanner(IAssemblyScanner<T> scanner)
        {
            this.scanner = scanner;
            return this;
        }

        public AssemblyScanningComposer<T> WithAssemblyScannerOptions<TScannerOptions>()
            where TScannerOptions : IAssemblyScannerOptions<T>
        {
            this.scannerOptionsType = typeof(TScannerOptions);
            return this;
        }

        public AssemblyScanningComposer<T> WithAssemblyScannerOptions(IAssemblyScannerOptions<T> options)
        {
            this.scannerOptions = options;
            return this;
        }
    }
}
