using System;

namespace Prise.Infrastructure
{
    public interface IRootPathProvider : IDisposable
    {
        string GetRootPath();
    }

    public class RootPathProvider : IRootPathProvider
    {
        private readonly string rootPath;
        private bool disposed = false;

        public RootPathProvider(string rootPath)
        {
            this.rootPath = rootPath;
        }

        public string GetRootPath() => this.rootPath;

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