using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Prise.Core;
using Prise.Utils;

namespace Prise.AssemblyLoading
{
    public class DefaultAssemblyLoader : IAssemblyLoader, IDisposable
    {
        private readonly Func<IAssemblyLoadContext> assemblyLoadContextFactory;
        protected ConcurrentDictionary<string, IAssemblyLoadContext> loadContexts;
        protected ConcurrentDictionary<string, WeakReference> loadContextReferences;

        public DefaultAssemblyLoader(Func<IAssemblyLoadContext> assemblyLoadContextFactory)
        {
            this.assemblyLoadContextFactory = assemblyLoadContextFactory.ThrowIfNull(nameof(assemblyLoadContextFactory));
            this.loadContexts = new ConcurrentDictionary<string, IAssemblyLoadContext>();
            this.loadContextReferences = new ConcurrentDictionary<string, WeakReference>();
        }

        public virtual Task<IAssemblyShim> Load(IPluginLoadContext pluginLoadContext)
        {
            if (pluginLoadContext == null)
                throw new ArgumentNullException(nameof(pluginLoadContext));

            var fullPathToAssembly = pluginLoadContext.FullPathToPluginAssembly.ThrowIfNullOrEmpty(nameof(pluginLoadContext.FullPathToPluginAssembly));

            if (!Path.IsPathRooted(fullPathToAssembly))
                throw new AssemblyLoadingException($"FullPathToPluginAssembly {pluginLoadContext.FullPathToPluginAssembly} is not rooted, this must be a absolute path!");

            var loadContext = this.assemblyLoadContextFactory();
            this.loadContexts[fullPathToAssembly] = loadContext;
            this.loadContextReferences[fullPathToAssembly] = new System.WeakReference(loadContext);
            return loadContext.LoadPluginAssembly(pluginLoadContext);
        }

        public virtual Task Unload(IPluginLoadContext pluginLoadContext)
        {
            if (pluginLoadContext == null)
                throw new ArgumentNullException(nameof(pluginLoadContext));

            var fullPathToAssembly = pluginLoadContext.FullPathToPluginAssembly.ThrowIfNullOrEmpty(nameof(pluginLoadContext.FullPathToPluginAssembly));

            if (!Path.IsPathRooted(fullPathToAssembly))
                throw new AssemblyLoadingException($"FullPathToPluginAssembly {pluginLoadContext.FullPathToPluginAssembly} is not rooted, this must be a absolute path!");

            UnloadContexts(fullPathToAssembly);

            return Task.CompletedTask;
        }

        protected virtual void UnloadContexts(string fullPathToAssembly)
        {
            UnloadAndCleanup();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        protected virtual void UnloadAndCleanup(string fullPathToAssembly = null)
        {
            if (this.loadContexts != null)
            {
                var loadContexts = String.IsNullOrEmpty(fullPathToAssembly) ?
                                            this.loadContexts.Where(c => c.Key == fullPathToAssembly).Select(c => c.Key) :
                                            this.loadContexts.Select(c => c.Key);
                foreach (var key in loadContexts)
                {
                    this.loadContexts[key].Unload();
                    this.loadContexts.TryRemove(key, out _);
                }
            }

            if (this.loadContextReferences != null)
            {
                var loadContextReferences = String.IsNullOrEmpty(fullPathToAssembly) ?
                                            this.loadContextReferences.Where(c => c.Key == fullPathToAssembly).Select(c => c.Key) :
                                            this.loadContextReferences.Select(c => c).Select(c => c.Key);
                foreach (var key in loadContextReferences)
                {
                    var reference = this.loadContextReferences[key];
                    for (int i = 0; reference.IsAlive && (i < 10); i++)
                    {
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                        GC.WaitForPendingFinalizers();
                    }
                }
            }
        }

        protected virtual void DisposeAndUnloadContexts()
        {
            // Clean up all
            this.UnloadAndCleanup();

            this.loadContexts?.Clear();
            this.loadContexts = null;

            this.loadContextReferences?.Clear();
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