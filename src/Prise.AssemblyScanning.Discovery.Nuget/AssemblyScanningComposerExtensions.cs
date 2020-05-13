namespace Prise.AssemblyScanning.Discovery.Nuget
{
    public static class AssemblyScanningComposerExtensions
    {
        public static AssemblyScanningComposer<T> UseNugetPackageDiscovery<T>(this AssemblyScanningComposer<T> options)
        {
            return options.WithAssemblyScanner<NugetPackageAssemblyScanner<T>>();
        }
    }
}
