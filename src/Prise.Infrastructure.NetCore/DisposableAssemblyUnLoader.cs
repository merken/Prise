using System;
using System.Threading.Tasks;

namespace Prise.Infrastructure.NetCore
{
    public abstract class DisposableAssemblyUnLoader : IDisposable
    {
        protected IAssemblyLoadContext loadContext;
        protected WeakReference assemblyLoadContextReference;

        protected bool disposed = false;
        public virtual void Unload()
        {
#if NETCORE3_0
            if (this.loadContext != null)
                this.loadContext.Unload();
#endif
            for (int i = 0; assemblyLoadContextReference.IsAlive && (i < 10); i++)
            {
                GC.ReRegisterForFinalize(assemblyLoadContextReference.Target);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            this.loadContext.Dispose();
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


            this.loadContext.Dispose();
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
                if (this.loadContext != null)
                    this.loadContext.Unload();
#endif
                this.loadContext.Dispose();
                this.loadContext = null;

                // TODO swithc around?
                for (int i = 0; assemblyLoadContextReference.IsAlive && (i < 10); i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                GC.Collect(); // collects all unused memory
                GC.WaitForPendingFinalizers(); // wait until GC has finished its work
                GC.Collect();
            }
            this.disposed = true;
        }
    }
}
