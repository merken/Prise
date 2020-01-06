using Prise.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prise.AssemblyScanning;

namespace Prise
{
    public class DefaultAssemblyScannerOptions<T> : IAssemblyScannerOptions<T>
    {
        private readonly IPluginPathProvider<T> pluginPathProvider;
        private readonly IRuntimePlatformContext runtimePlatformContext;

        public DefaultAssemblyScannerOptions(IPluginPathProvider<T> pluginPathProvider, IRuntimePlatformContext runtimePlatformContext)
        {
            this.pluginPathProvider = pluginPathProvider;
            this.runtimePlatformContext = runtimePlatformContext;
        }

        public string PathToScan => this.pluginPathProvider.GetPluginPath();

        public IEnumerable<string> FileTypesToScan => runtimePlatformContext.GetPluginDependencyNames("*");
    }

    public class DefaultAssemblyScanner<T> : IAssemblyScanner<T>
    {
        protected readonly IAssemblyScannerOptions<T> options;
        protected readonly IPluginAssemblyNameProvider<T> pluginAssemblyNameProvider;
        protected bool disposed = false;

        public DefaultAssemblyScanner(IAssemblyScannerOptions<T> options, IPluginAssemblyNameProvider<T> pluginAssemblyNameProvider)
        {
            this.options = options;
            this.pluginAssemblyNameProvider = pluginAssemblyNameProvider;
        }

        public void Dispose()
        {
            this.disposed = true;
        }

        public virtual Task<IEnumerable<AssemblyScanResult<T>>> Scan()
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
