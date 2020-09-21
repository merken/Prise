using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Prise.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Prise.AssemblyLoading
{
    public class DefaultAssemblyLoader : IAssemblyLoader, IDisposable
    {
        private readonly IServiceProvider serviceProvider;
        protected ConcurrentDictionary<string, IServiceScope> serviceScopes;
        protected ConcurrentDictionary<string, IAssemblyLoadContext> loadContexts;
        protected ConcurrentDictionary<string, WeakReference> loadContextReferences;

        public DefaultAssemblyLoader(IServiceProvider serviceProvider = null)
        {
            this.serviceProvider = serviceProvider;
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

            var scope = this.serviceProvider.CreateScope();
            var loadContext = scope.ServiceProvider.GetRequiredService<IAssemblyLoadContext>();

            this.serviceScopes[fullPathToAssembly] = scope;
            this.loadContexts[fullPathToAssembly] = loadContext;
            this.loadContextReferences[fullPathToAssembly] = new System.WeakReference(loadContext);

            return loadContext.LoadPluginAssembly(pluginLoadContext);
        }

        public virtual async Task Unload(IPluginLoadContext loadContext)
        {
            UnloadScopesAndContexts(loadContext.FullPathToPluginAssembly);
        }

        protected virtual void UnloadScopesAndContexts(string fullPathToAssembly)
        {
            var pluginName = Path.GetFileNameWithoutExtension(fullPathToAssembly);
            var loadContextKeys = this.loadContexts.Keys.Where(k => k == fullPathToAssembly);

            foreach (var key in this.loadContexts.Keys.Where(k => k == fullPathToAssembly))
                this.loadContexts[key].Unload();
                // LoadContextReferences ?

            foreach (var key in this.serviceScopes.Keys.Where(k => k == fullPathToAssembly))
                this.serviceScopes[key].Dispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        protected virtual void DisposeAndUnloadContexts()
        {
            if (this.loadContexts != null)
                foreach (var key in this.loadContexts.Keys)
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

            if (this.serviceScopes != null)
                foreach (var key in this.serviceScopes.Keys)
                    this.serviceScopes[key].Dispose();

            this.serviceScopes.Clear();
            this.serviceScopes = null;

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