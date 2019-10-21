using System;
using System.Linq;
using System.Threading.Tasks;

namespace Prise.Infrastructure.NetCore
{
    public class PrisePluginLoader<T> : PluginLoader, IPluginLoader<T>
    {
        private readonly IPluginLoadOptions<T> pluginLoadOptions;
        private bool disposed = false;

        public PrisePluginLoader(IPluginLoadOptions<T> pluginLoadOptions)
        {
            this.pluginLoadOptions = pluginLoadOptions;
        }

        public virtual async Task<T> Load()
        {
            return (await this.LoadPluginsOfType<T>(this.pluginLoadOptions)).First();
        }

        public virtual async Task<T[]> LoadAll()
        {
            return (await this.LoadPluginsOfType<T>(this.pluginLoadOptions));
        }

        public virtual async Task Unload()
        {
            await this.pluginLoadOptions.AssemblyLoader.Unload();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                // Disposes all configured services for the PluginLoadOptions
                // Including the AssemblyLoader
                this.pluginLoadOptions.Dispose();

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