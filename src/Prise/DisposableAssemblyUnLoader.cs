using System;
using System.Threading.Tasks;
using Prise.Infrastructure;

namespace Prise
{
    public abstract class DisposableAssemblyUnLoader : IDisposable
    {
        protected IAssemblyLoadContext loadContext;
        protected WeakReference assemblyLoadContextReference;
        protected bool disposed = false;

        public virtual void Unload()
        {
            DisposeAndUnloadContext();
        }

        public async virtual Task UnloadAsync()
        {
            DisposeAndUnloadContext();
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
                DisposeAndUnloadContext();
            }
            this.disposed = true;
        }

        protected virtual void DisposeAndUnloadContext()
        {
#if NETCORE3_0
             this.loadContext?.Unload();
#endif
            this.loadContext?.Dispose();
            this.loadContext = null;

            // See https://docs.microsoft.com/en-us/dotnet/standard/assembly/unloadability#use-collectible-assemblyloadcontext
            for (int i = 0; assemblyLoadContextReference.IsAlive && (i < 10); i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            GC.Collect(); // collects all unused memory
            GC.WaitForPendingFinalizers(); // wait until GC has finished its work
            GC.Collect();
        }
    }
}
