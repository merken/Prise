using System.IO;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Prise.Infrastructure;

namespace Prise.Mvc
{
    public class PrisePluginViewsAssemblyFileProvider<T> : IFileProvider
    {
        protected PhysicalFileProvider webRootFileProvider;
        public PrisePluginViewsAssemblyFileProvider(string hostingRootPath)
        {
            this.webRootFileProvider = new PhysicalFileProvider(hostingRootPath);
        }

        private IPluginCache<T> GetLoadedPluginsCache()
        {
            return StaticPluginCacheAccessor<T>.CurrentCache;
        }

        private IFileProvider GetPluginFileProvider(string subpath)
        {
            var cache = GetLoadedPluginsCache();
            if (cache == null)
                return null;

            foreach (var loadedPlugin in cache.GetAll())
            {
                var pluginAssemblyName = Path.GetFileNameWithoutExtension(loadedPlugin.GetName().Name);
                var executingFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (File.Exists(Path.Join(Path.Combine(executingFolder, "Plugins", pluginAssemblyName), subpath)))
                {
                    return new PhysicalFileProvider(Path.Combine(executingFolder, "Plugins", pluginAssemblyName));
                }
            }
            return null;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var pluginFileProvider = GetPluginFileProvider(subpath);
            if (pluginFileProvider != null)
                return pluginFileProvider.GetDirectoryContents(subpath);
            return this.webRootFileProvider.GetDirectoryContents(subpath);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var pluginFileProvider = GetPluginFileProvider(subpath);
            if (pluginFileProvider != null)
                return pluginFileProvider.GetFileInfo(subpath);
            return this.webRootFileProvider.GetFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return this.webRootFileProvider.Watch(filter);
        }
    }
}
