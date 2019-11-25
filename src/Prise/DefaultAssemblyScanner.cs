using Prise.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prise.AssemblyScanning;

namespace Prise
{
    public class DefaultAssemblyScannerOptions<T> : IAssemblyScannerOptions<T>
    {
        private readonly IPluginPathProvider pluginPathProvider;
        private readonly IRuntimePlatformContext runtimePlatformContext;

        public DefaultAssemblyScannerOptions(IPluginPathProvider pluginPathProvider, IRuntimePlatformContext runtimePlatformContext)
        {
            this.pluginPathProvider = pluginPathProvider;
            this.runtimePlatformContext = runtimePlatformContext;
        }

        public string PathToScan => pluginPathProvider.GetPluginPath();

        public IEnumerable<string> FileTypesToScan => runtimePlatformContext.GetPluginDependencyNames("*");
    }

    public class DefaultAssemblyScanner<T> : IAssemblyScanner<T>
    {
        private readonly IAssemblyScannerOptions<T> options;
        private readonly IPluginAssemblyNameProvider pluginAssemblyNameProvider;

        public DefaultAssemblyScanner(IAssemblyScannerOptions<T> options, IPluginAssemblyNameProvider pluginAssemblyNameProvider)
        {
            this.options = options;
            this.pluginAssemblyNameProvider = pluginAssemblyNameProvider;
        }

        public Task<IEnumerable<AssemblyScanResult<T>>> Scan()
        {
            return Task.FromResult(new[] {
                new AssemblyScanResult<T> {
                    AssemblyName = this.pluginAssemblyNameProvider.GetAssemblyName(),
                    AssemblyPath = this.options.PathToScan
                }
            }.AsEnumerable());
        }
    }
}
