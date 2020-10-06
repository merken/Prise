using System;
using System.Reflection;
using Prise.Utils;

namespace Prise.AssemblyScanning
{
    public class DefaultMetadataLoadContext : IMetadataLoadContext
    {
        private readonly MetadataLoadContext loadContext;
        public DefaultMetadataLoadContext(string fullPathToAssembly)
        {
            this.loadContext = new MetadataLoadContext(new DefaultAssemblyResolver(fullPathToAssembly.ThrowIfNullOrEmpty(nameof(fullPathToAssembly))));
        }

        public IAssemblyShim LoadFromAssemblyName(string assemblyName) => new PriseAssembly(loadContext.LoadFromAssemblyName(assemblyName));

        public void Dispose()
        {
            this.loadContext?.Dispose();
        }
    }
}