using System;
using System.Linq;
using System.Threading.Tasks;

namespace Prise.Infrastructure.NetCore
{
    public class SinglePluginLoader<T> : PluginLoader, IPluginLoader<T>
    {
        private readonly IPluginLoadOptions<T> pluginLoadOptions;

        public SinglePluginLoader(IPluginLoadOptions<T> pluginLoadOptions)
        {
            this.pluginLoadOptions = pluginLoadOptions;
        }

        public virtual async Task<T> Load()
        {
            return (await this.LoadPluginsOfType<T>(this.pluginLoadOptions)).First();
        }

        public virtual async Task<T[]> LoadAll()
        {
            throw new System.NotImplementedException("Loading multiple plugins is not supported in this loader");
        }

        public virtual async Task Unload()
        {
            await this.pluginLoadOptions.AssemblyLoader.Unload();
        }
    }
}