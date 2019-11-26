using Contract;
using Prise.AssemblyScanning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Function.Infrastructure
{
    public class FunctionPluginStaticAssemblyScanner : IAssemblyScanner<IHelloPlugin>
    {
        private readonly string assemblyName;
        private readonly string path;

        public FunctionPluginStaticAssemblyScanner(string assemblyName, string path = null)
        {
            this.assemblyName = assemblyName;
            this.path = path;
        }

        public Task<IEnumerable<AssemblyScanResult<IHelloPlugin>>> Scan()
        {
            return Task.FromResult(new[] {
                new AssemblyScanResult<IHelloPlugin> {
                    AssemblyName = assemblyName,
                    AssemblyPath = path
                }
            }.AsEnumerable());
        }
    }
}
