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

    public class StaticAssemblyScanner<T> : IAssemblyScanner<T>
    {
        protected readonly string assemblyName;
        protected readonly string path;

        public StaticAssemblyScanner(string assemblyName, string path)
        {
            this.assemblyName = assemblyName;
            this.path = path;
        }

        public virtual Task<IEnumerable<AssemblyScanResult<T>>> Scan()
        {
            return Task.FromResult(new[] {
                new AssemblyScanResult<T> {
                    AssemblyName = this.assemblyName,
                    AssemblyPath = this.path
                }
            }.AsEnumerable());
        }
    }

    public class DefaultAssemblyScanner<T> : IAssemblyScanner<T>
    {
        protected readonly IAssemblyScannerOptions<T> options;
        protected readonly IPluginAssemblyNameProvider<T> pluginAssemblyNameProvider;

        public DefaultAssemblyScanner(IAssemblyScannerOptions<T> options, IPluginAssemblyNameProvider<T> pluginAssemblyNameProvider)
        {
            this.options = options;
            this.pluginAssemblyNameProvider = pluginAssemblyNameProvider;
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
