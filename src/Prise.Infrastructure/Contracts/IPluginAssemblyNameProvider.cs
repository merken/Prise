using System;

namespace Prise.Infrastructure
{
    public interface IPluginAssemblyNameProvider : IDisposable
    {
        string GetAssemblyName();
    }

    public class PluginAssemblyNameProvider : IPluginAssemblyNameProvider
    {
        private readonly string assemblyName;
        private bool disposed = false;

        public PluginAssemblyNameProvider(string assemblyName)
        {
            this.assemblyName = assemblyName;
        }

        public virtual string GetAssemblyName() => this.assemblyName;

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