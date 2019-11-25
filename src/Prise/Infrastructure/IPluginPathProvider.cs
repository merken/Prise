using System;
using System.Collections.Generic;
using System.Text;

namespace Prise.Infrastructure
{
    public interface IPluginPathProvider : IDisposable
    {
        string GetPluginPath();
    }

    public class DefaultPluginPathProvider : IPluginPathProvider
    {
        private readonly string pluginPath;
        private bool disposed = false;

        public DefaultPluginPathProvider(string pluginPath)
        {
            this.pluginPath = pluginPath;
        }

        public string GetPluginPath() => this.pluginPath;

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
