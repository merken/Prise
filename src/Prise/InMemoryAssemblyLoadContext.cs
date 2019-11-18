using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Prise
{
    /// <summary>
    /// This base class will load all assemblies in memory by default and is Collectable by default
    /// </summary>
    public abstract class InMemoryAssemblyLoadContext : AssemblyLoadContext
    {
        protected InMemoryAssemblyLoadContext()
#if NETCOREAPP3_0
            : base($"PriseInMemoryAssemblyLoadContext{Guid.NewGuid().ToString("N")}", isCollectible: true)
#endif
        { }

        public new Assembly LoadFromAssemblyPath(string path)
        {
            using (var stream = File.OpenRead(path))
                return LoadFromStream(stream);
        }
    }
}
