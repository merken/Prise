using Prise.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
            if (loadContexts != null)
            {
                IEnumerable<string> loadContextsList = loadContexts.Select(c => c.Key);

                if (string.IsNullOrEmpty(fullPathToAssembly))
                {
                    loadContextsList = loadContextsList.Where(c => c == fullPathToAssembly);
                }

                foreach (string key in loadContextsList)
                {
                    loadContexts[key].Unload();
                    loadContexts.TryRemove(key, out _);
                }
            }

            if (loadContextReferences != null)
            {
                IEnumerable<string> loadContextReferencesList = loadContextReferences.Select(c => c).Select(c => c.Key); ;

                if (string.IsNullOrEmpty(fullPathToAssembly))
                {
                    loadContextReferencesList = loadContextReferencesList.Where(c => c == fullPathToAssembly);
                }

                foreach (string key in loadContextReferencesList)
                {
                    WeakReference reference = loadContextReferences[key];

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
            UnloadAndCleanup();

            loadContexts?.Clear();
            loadContexts = null;

            loadContextReferences?.Clear();
            loadContextReferences = null;
        }

        protected volatile bool disposed;

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            DisposeAndUnloadContexts();
        }
    }
}