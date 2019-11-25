using Prise.Infrastructure;
using System;
using System.Diagnostics;

namespace Prise
{
    [DebuggerDisplay("{GetDependencyPath()}")]
    public class DependencyPathProvider<T> : IDependencyPathProvider<T>
    {
        private readonly string dependencyPath;
        private bool disposed = false;

        public DependencyPathProvider(string dependencyPath)
        {
            this.dependencyPath = dependencyPath;
        }

        public string GetDependencyPath() => this.dependencyPath;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                // Nothing to do here
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
