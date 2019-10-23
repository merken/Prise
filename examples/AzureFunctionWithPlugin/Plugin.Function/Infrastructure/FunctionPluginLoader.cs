using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Contract;
using Prise.Infrastructure;
using Prise.Infrastructure.NetCore;
using Prise.Infrastructure.NetCore.Contracts;

namespace Plugin.Function.Infrastructure
{
    public class FunctionPluginLoader : PluginLoader, IPluginLoader<IHelloPlugin>, IPluginResolver<IHelloPlugin>
    {
        private bool disposed = false;
        private string componentName;
        private readonly IHttpClientFactory factory;
        private readonly IPluginServerOptions pluginServerOptions;
        private readonly IPluginLoadOptions<IHelloPlugin> pluginLoadOptions;

        public FunctionPluginLoader(
            IPluginLoadOptions<IHelloPlugin> pluginLoadOptions, 
            IHttpClientFactory factory,
            IPluginServerOptions pluginServerOptions)
        {
            this.pluginLoadOptions = pluginLoadOptions;
            this.factory = factory;
            this.pluginServerOptions = pluginServerOptions;
        }

        internal void SetComponentToLoad(string componentName)
        {
            this.componentName = componentName;
        }

        private PluginLoadOptions<IHelloPlugin> GetOptions() =>
            new PluginLoadOptions<IHelloPlugin>(
                this.pluginLoadOptions.RootPathProvider,
                this.pluginLoadOptions.SharedServicesProvider,
                this.pluginLoadOptions.Activator,
                this.pluginLoadOptions.ParameterConverter,
                this.pluginLoadOptions.ResultConverter,
                new NetworkAssemblyLoader<IHelloPlugin>(
                    new NetworkAssemblyLoaderOptions($"{this.pluginServerOptions.PluginServerUrl}/{this.componentName}"), this.factory),
                new PluginAssemblyNameProvider($"{this.componentName}.dll")
            );

        public virtual async Task<IHelloPlugin> Load()
        {
            return (await this.LoadPluginsOfTypeAsync<IHelloPlugin>(GetOptions())).First();
        }

        IHelloPlugin IPluginResolver<IHelloPlugin>.Load()
        {
            return this.LoadPluginsOfType<IHelloPlugin>(GetOptions()).First();
        }

        public virtual async Task<IHelloPlugin[]> LoadAll()
        {
            return (await this.LoadPluginsOfTypeAsync<IHelloPlugin>(GetOptions()));
        }

        IHelloPlugin[] IPluginResolver<IHelloPlugin>.LoadAll()
        {
            return this.LoadPluginsOfType<IHelloPlugin>(GetOptions());
        }

        public virtual async Task Unload()
        {
            await this.pluginLoadOptions.AssemblyLoader.UnloadAsync();
        }

        void IPluginResolver<IHelloPlugin>.Unload()
        {
            this.pluginLoadOptions.AssemblyLoader.Unload();
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