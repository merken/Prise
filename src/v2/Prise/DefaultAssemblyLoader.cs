using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;

namespace Prise.V2
{
    public class DefaultAssemblyLoader : IPluginAssemblyLoader, IDisposable
    {
        protected ConcurrentDictionary<string, IAssemblyLoadContext> loadContexts;
        protected ConcurrentDictionary<string, WeakReference> loadContextReferences;
        protected bool disposed = false;

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
                throw new AssemblyLoadException($"FullPathToPluginAssembly {pluginLoadContext.FullPathToPluginAssembly} is not rooted, this must be a absolute path!");

            var loadContext = new DefaultAssemblyLoadContext();
            this.loadContexts[fullPathToAssembly] = loadContext;
            this.loadContextReferences[fullPathToAssembly] = new System.WeakReference(loadContext);
            return loadContext.LoadPluginAssembly(pluginLoadContext);
        }

        public virtual async Task Unload(IPluginLoadContext loadContext)
        {
            UnloadContext(loadContext.FullPathToPluginAssembly);
        }

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
    }
}