using System;
using Prise.Infrastructure;

namespace Prise
{
    public class PluginContextAsDependencyPathProvider<T> : IDependencyPathProvider<T>
    {
        private bool disposed = false;

        /// <summary>
        /// Returning an empty dependency path will result in loading dependencies from the IPluginLoadContext<T> path
        /// </summary>
        /// <returns></returns>
        public string GetDependencyPath() => String.Empty;

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
