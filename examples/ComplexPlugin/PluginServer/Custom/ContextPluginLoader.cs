using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Prise.Infrastructure;
using Prise.Infrastructure.NetCore;
using Prise.Infrastructure.NetCore.Contracts;

namespace PluginServer.Custom
{
    public class ContextPluginLoader<T> : PluginLoader, IPluginLoader<T>
    {
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IPluginLoadOptions<T> pluginLoadOptions;

        public ContextPluginLoader(IHttpContextAccessor contextAccessor, IPluginLoadOptions<T> pluginLoadOptions)
        {
            this.contextAccessor = contextAccessor;
            this.pluginLoadOptions = pluginLoadOptions;
        }
        public async Task<T> Load()
        {
            return (await LoadPluginsOfType<T>(GetLoadOptionsFromContext())).First();
        }

        public Task<T[]> LoadAll()
        {
            return LoadPluginsOfType<T>(GetLoadOptionsFromContext());
        }

        private PluginLoadOptions<T> GetLoadOptionsFromContext()
        {
            var pluginHeader = this.contextAccessor.HttpContext.Request.Headers["PluginType"];
            var assemblyToLoadFrom = $"{pluginHeader}.dll";
            var pluginPath = $"Plugins\\{pluginHeader}";

            return new PluginLoadOptions<T>(
                this.pluginLoadOptions.RootPathProvider,
                this.pluginLoadOptions.Activator,
                this.pluginLoadOptions.ParameterConverter,
                this.pluginLoadOptions.ResultConverter,
                new LocalDiskAssemblyLoader<T>(this.pluginLoadOptions.RootPathProvider, new LocalAssemblyLoaderOptions(pluginPath)),
                this.pluginLoadOptions.PluginAssemblyNameProvider
            );
        }
    }
}