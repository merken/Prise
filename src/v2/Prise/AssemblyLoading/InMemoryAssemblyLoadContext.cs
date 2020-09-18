using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Prise.AssemblyLoading
{
    /// <summary>
    /// This base class will load all assemblies in memory by default and is Collectible by default
    /// </summary>
    public abstract class InMemoryAssemblyLoadContext : AssemblyLoadContext
    {
        protected bool isCollectible = false;
        protected InMemoryAssemblyLoadContext() { }

        protected InMemoryAssemblyLoadContext(bool isCollectible) : base($"InMemoryAssemblyLoadContext{Guid.NewGuid().ToString("N")}", isCollectible: isCollectible)
        {
            this.isCollectible = isCollectible;
        }

        public new Assembly LoadFromAssemblyPath(string path)
        {
            try
            {
                using (var stream = File.OpenRead(path))
                    return LoadFromStream(stream);
            }
            catch (InvalidOperationException ex)
            {
                throw new AssemblyLoadingException($"Assembly at {path} could not be loaded (locked dll file)", ex);
            }
        }
    }

}