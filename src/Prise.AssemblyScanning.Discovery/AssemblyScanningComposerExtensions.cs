namespace Prise.AssemblyScanning.Discovery
{
    public static class AssemblyScanningComposerExtensions
    {
        public static AssemblyScanningComposer<T> UseDiscovery<T>(this AssemblyScanningComposer<T> options)
        {
            return options.WithAssemblyScanner<DiscoveryAssemblyScanner<T>>();
        }
    }
}
