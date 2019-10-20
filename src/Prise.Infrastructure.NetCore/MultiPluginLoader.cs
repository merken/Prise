using System;
using System.Linq;
using System.Threading.Tasks;

namespace Prise.Infrastructure.NetCore
{
    public class MultiPluginLoader<T> : PluginLoader, IPluginLoader<T>
    {
        private readonly IPluginLoadOptions<T> pluginLoadOptions;

        public MultiPluginLoader(IPluginLoadOptions<T> pluginLoadOptions)
        {
            this.pluginLoadOptions = pluginLoadOptions;
        }

        public virtual async Task<T> Load()
        {
            return (await this.LoadPluginsOfType<T>(this.pluginLoadOptions)).First();
        }

        public virtual async Task<T[]> LoadAll()
        {
            return await this.LoadPluginsOfType<T>(this.pluginLoadOptions);
        }

        public virtual async Task Unload()
        {
            await this.pluginLoadOptions.AssemblyLoader.Unload();
        }
    }
}