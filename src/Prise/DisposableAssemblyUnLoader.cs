using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Prise.Infrastructure;

namespace Prise
{
    public struct LoadedPluginKey
    {
        public string Path { get; set; }
        public string PluginAssemblyName { get; set; }

        public LoadedPluginKey(IPluginLoadContext context)
        {
            this.Path = context.PluginAssemblyPath;
            this.PluginAssemblyName = System.IO.Path.GetFileNameWithoutExtension(context.PluginAssemblyName);
        }
    }

    public abstract class DisposableAssemblyUnLoader : IDisposable
    {
        protected ConcurrentDictionary<LoadedPluginKey, IAssemblyLoadContext> loadContexts;
        protected ConcurrentDictionary<LoadedPluginKey, WeakReference> loadContextReferences;
        protected bool disposed = false;
        protected UnloadStrategy unloadStrategy;

        protected DisposableAssemblyUnLoader(UnloadStrategy unloadStrategy)
        {
            this.loadContexts = new ConcurrentDictionary<LoadedPluginKey, IAssemblyLoadContext>();
            this.loadContextReferences = new ConcurrentDictionary<LoadedPluginKey, WeakReference>();
            this.unloadStrategy = unloadStrategy;
        }

        public virtual void UnloadAll()
        {
            DisposeAndUnloadContexts();
        }

        public async virtual Task UnloadAllAsync()
        {
            DisposeAndUnloadContexts();
        }

        public virtual void Unload(string pluginAssemblyName)
        {
            UnloadContext(pluginAssemblyName);
        }

        public async virtual Task UnloadAsync(string pluginAssemblyName)
        {
            UnloadContext(pluginAssemblyName);
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

        protected virtual void UnloadContext(string pluginAssemblyName)
        {
            var pluginName = Path.GetFileNameWithoutExtension(pluginAssemblyName);
            var loadContextKeys = this.loadContexts.Keys.Where(k => k.PluginAssemblyName == pluginAssemblyName);

            foreach (var key in loadContextKeys)
            {
                var loadContext = this.loadContexts[key];
#if NETCORE3_0 || NETCORE3_1
                loadContext.Unload();
#endif
                loadContext.Dispose();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        protected virtual void DisposeAndUnloadContexts()
        {
            if (loadContexts != null)
                foreach (var loadContext in loadContexts.Values)
                {
#if NETCORE3_0 || NETCORE3_1
                    loadContext.Unload();
#endif
                    loadContext.Dispose();
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
