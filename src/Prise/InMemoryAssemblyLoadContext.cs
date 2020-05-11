using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Prise
{
    /// <summary>
    /// This base class will load all assemblies in memory by default and is Collectible by default
    /// </summary>
    public abstract class InMemoryAssemblyLoadContext : AssemblyLoadContext
    {
        protected bool isCollectible = false;
        protected InMemoryAssemblyLoadContext() { }

        // Provide the opt-in for collectible assemblies in netcore 3
#if NETCORE3_0 || NETCORE3_1
        protected InMemoryAssemblyLoadContext(bool isCollectible) : base($"PriseInMemoryAssemblyLoadContext{Guid.NewGuid().ToString("N")}", isCollectible: isCollectible)
        {
            this.isCollectible = isCollectible;
        }
#endif

        public new Assembly LoadIntoMemory(string path)
        {
            using (var stream = File.OpenRead(path))
                return LoadFromStream(stream);
        }
    }
}
