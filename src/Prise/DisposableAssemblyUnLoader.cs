using System;
using System.Threading;
using System.Threading.Tasks;
using Prise.Infrastructure;

namespace Prise
{
    public abstract class DisposableAssemblyUnLoader : IDisposable
    {
        protected IAssemblyLoadContext loadContext;

        protected bool disposed = false;
        public virtual void Unload()
        {
#if NETCORE3_0
            this.loadContext?.Unload();
#endif
            this.loadContext?.Dispose();
            this.loadContext = null;

            GC.Collect(); // collects all unused memory
            GC.WaitForPendingFinalizers(); // wait until GC has finished its work
            GC.Collect();
        }

        public async virtual Task UnloadAsync()
        {
#if NETCORE3_0
            if (this.loadContext != null)
                this.loadContext.Unload();
#endif
            this.loadContext?.Dispose();
            this.loadContext = null;
            GC.Collect(); // collects all unused memory
            GC.WaitForPendingFinalizers(); // wait until GC has finished its work
            GC.Collect();
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
#if NETCORE3_0
                this.loadContext?.Unload();
#endif
                this.loadContext?.Dispose();
                this.loadContext = null;

                GC.Collect(); // collects all unused memory
                GC.WaitForPendingFinalizers(); // wait until GC has finished its work
                GC.Collect();
            }
            this.disposed = true;
        }
    }
}
