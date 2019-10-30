using System;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace Prise.Infrastructure.NetCore
{
    public abstract class DisposableAssemblyUnLoader : IDisposable
    {
        protected AssemblyLoadContext loadContext;
        protected bool disposed = false;
        public virtual void Unload()
        {
#if NETCORE3_0
            if (loadContext != null)
                loadContext.Unload();
#endif
            GC.Collect(); // collects all unused memory
            GC.WaitForPendingFinalizers(); // wait until GC has finished its work
            GC.Collect();
        }

        public async virtual Task UnloadAsync()
        {
#if NETCORE3_0
            if (loadContext != null)
                loadContext.Unload();
#endif
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
            if (loadContext != null)
                loadContext.Unload();
#endif
                GC.Collect(); // collects all unused memory
                GC.WaitForPendingFinalizers(); // wait until GC has finished its work
                GC.Collect();
            }
            this.disposed = true;
        }
    }
}
