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
        protected bool disposed = false;
        protected bool disposing = false;
        protected bool isCollectible = false;
        protected InMemoryAssemblyLoadContext() { }

        protected InMemoryAssemblyLoadContext(bool isCollectible)
#if SUPPORTS_UNLOADING
            : base($"InMemoryAssemblyLoadContext{Guid.NewGuid().ToString("N")}", isCollectible: isCollectible)
#endif
        {
#if SUPPORTS_UNLOADING
            this.isCollectible = isCollectible;
#else
            this.isCollectible = false;
#endif
        }

        public new Assembly LoadFromAssemblyPath(string path)
        {
            // This fixes the issue where the ALC is still alive and utilized in the host
            if (this.disposed || this.disposing)
                return null;

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