using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Prise.Caching;

namespace Prise.Mvc
{
    public class DefaultPrisePluginViewsAssemblyFileProvider : IFileProvider
    {
        protected readonly PhysicalFileProvider webRootFileProvider;
        protected readonly string pathToPlugins;
        public DefaultPrisePluginViewsAssemblyFileProvider(string hostingRootPath, string pathToPlugins)
        {
            if (!Path.IsPathRooted(pathToPlugins))
                throw new ArgumentException($"{nameof(pathToPlugins)} must be rooted (absolute path).");

            this.pathToPlugins = pathToPlugins;
            this.webRootFileProvider = new PhysicalFileProvider(hostingRootPath);
        }

        private IPluginCache GetLoadedPluginsCache()
        {
            return DefaultStaticPluginCacheAccessor.CurrentCache;
        }

        private IFileProvider GetPluginFileProvider(string subpath)
        {
            var cache = GetLoadedPluginsCache();
            if (cache == null)
                return null;

            foreach (var loadedPlugin in cache.GetAll())
            {
                var pluginAssemblyName = loadedPlugin.AssemblyShim.Assembly.GetName().Name;
                var pathToPlugin = Path.Combine(pathToPlugins, pluginAssemblyName);
                var pathCandidate = Path.Combine(pathToPlugin, SanitizeSubPath(subpath));
                if (File.Exists(pathCandidate))
                    return new PhysicalFileProvider(pathToPlugin);
            }
            return null;
        }

        private string SanitizeSubPath(string subPath)
        {
            if (subPath.StartsWith('/'))
                return subPath.Substring(1);
            return subPath;
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
