using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Prise.Core;

namespace Prise.AssemblyLoading
{
    public class DefaultAssemblyLoader : IAssemblyLoader, IDisposable
    {
        protected ConcurrentDictionary<string, IAssemblyLoadContext> loadContexts;
        protected ConcurrentDictionary<string, WeakReference> loadContextReferences;

        public DefaultAssemblyLoader()
        {
            this.loadContexts = new ConcurrentDictionary<string, IAssemblyLoadContext>();
            this.loadContextReferences = new ConcurrentDictionary<string, WeakReference>();
        }

        public virtual Task<IAssemblyShim> Load(IPluginLoadContext pluginLoadContext)
        {
            if (pluginLoadContext == null)
                throw new ArgumentNullException(nameof(pluginLoadContext));

            var fullPathToAssembly = pluginLoadContext.FullPathToPluginAssembly;

            if (!Path.IsPathRooted(fullPathToAssembly))
                throw new AssemblyLoadingException($"FullPathToPluginAssembly {pluginLoadContext.FullPathToPluginAssembly} is not rooted, this must be a absolute path!");

            var loadContext = new DefaultAssemblyLoadContext();
            this.loadContexts[fullPathToAssembly] = loadContext;
            this.loadContextReferences[fullPathToAssembly] = new System.WeakReference(loadContext);
            return loadContext.LoadPluginAssembly(pluginLoadContext);
        }

        public virtual async Task Unload(IPluginLoadContext loadContext)
        {
            UnloadContext(loadContext.FullPathToPluginAssembly);
        }

        protected virtual void UnloadContext(string fullPathToAssembly)
        {
            var pluginName = Path.GetFileNameWithoutExtension(fullPathToAssembly);
            var loadContextKeys = this.loadContexts.Keys.Where(k => k == fullPathToAssembly);

            foreach (var key in loadContextKeys)
            {
                var loadContext = this.loadContexts[key];
                loadContext.Unload();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        protected virtual void DisposeAndUnloadContexts()
        {
            if (loadContexts != null)
                foreach (var key in loadContexts.Keys)
                {
                    var loadContext = this.loadContexts[key];
                    loadContext.Unload();
                }

            this.loadContexts.Clear();
            this.loadContexts = null;

            if (this.loadContextReferences != null)
                foreach (var reference in this.loadContextReferences.Values)
                {
                    // https://docs.microsoft.com/en-us/dotnet/standard/assembly/unloadability#use-collectible-assemblyloadcontext
                    for (int i = 0; reference.IsAlive && (i < 10); i++)
                    {
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                        GC.WaitForPendingFinalizers();
                    }
                }

            this.loadContextReferences.Clear();
            this.loadContextReferences = null;

            GC.Collect(); // collects all unused memory
            GC.WaitForPendingFinalizers(); // wait until GC has finished its work
            GC.Collect();
        }

        protected bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                DisposeAndUnloadContexts();
            }
            this.disposed = true;
        }
    }
}