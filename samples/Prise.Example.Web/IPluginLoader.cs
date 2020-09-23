using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Prise.DependencyInjection;
using Prise.Utils;
using Prise.Example.Contract;

namespace Prise.Example.Web
{
    public interface IPluginLoader
    {
        Task<IEnumerable<Prise.Core.AssemblyScanResult>> FindPlugins<T>(string pathToPlugins);
        IAsyncEnumerable<T> LoadPlugins<T>(Prise.Core.AssemblyScanResult plugin);
    }

    public class PluginLoader : IPluginLoader
    {
        private readonly IHttpContextAccessorService httpContextAccessorService;
        private readonly Prise.AssemblyScanning.IAssemblyScanner assemblyScanner;
        private readonly Prise.Core.IPluginTypeSelector pluginTypeSelector;
        private readonly Prise.AssemblyLoading.IAssemblyLoader assemblyLoader;
        private readonly Prise.Activation.IPluginActivator pluginActivator;

        public PluginLoader(IHttpContextAccessorService httpContextAccessorService,
                            Prise.AssemblyScanning.IAssemblyScanner assemblyScanner,
                            Prise.Core.IPluginTypeSelector pluginTypeSelector,
                            Prise.AssemblyLoading.IAssemblyLoader assemblyLoader,
                            Prise.Activation.IPluginActivator pluginActivator)
        {
            this.httpContextAccessorService = httpContextAccessorService;
            this.assemblyScanner = assemblyScanner;
            this.pluginTypeSelector = pluginTypeSelector;
            this.assemblyLoader = assemblyLoader;
            this.pluginActivator = pluginActivator;
        }

        public async Task<IEnumerable<Prise.Core.AssemblyScanResult>> FindPlugins<T>(string pathToPlugins)
        {
            return (await this.assemblyScanner.Scan(new Prise.AssemblyScanning.AssemblyScannerOptions
            {
                StartingPath = pathToPlugins,
                PluginType = typeof(T)
            }));
        }

        public async IAsyncEnumerable<T> LoadPlugins<T>(Prise.Core.AssemblyScanResult plugin)
        {
            var hostFramework = Prise.Utils.HostFrameworkUtils.GetHostframeworkFromHost();
            var servicesForPlugin = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            var pathToAssembly = Path.Combine(plugin.AssemblyPath, plugin.AssemblyName);
            var pluginLoadContext = Prise.Core.PluginLoadContext.DefaultPluginLoadContext(pathToAssembly, typeof(T), hostFramework);
            // This allows the loading of netstandard plugins
            pluginLoadContext.IgnorePlatformInconsistencies = true;
            pluginLoadContext.AddHostService<IHttpContextAccessorService>(servicesForPlugin, this.httpContextAccessorService); // Add this private field to collection

            var pluginAssembly = await this.assemblyLoader.Load(pluginLoadContext);
            var pluginTypes = this.pluginTypeSelector.SelectPluginTypes<T>(pluginAssembly);

            foreach (var pluginType in pluginTypes)
                yield return await this.pluginActivator.ActivatePlugin<T>(new Prise.Activation.DefaultPluginActivationOptions
                {
                    PluginType = pluginType,
                    PluginAssembly = pluginAssembly,
                    ParameterConverter = DefaultFactories.DefaultParameterConverter(),
                    ResultConverter = DefaultFactories.DefaultResultConverter(),
                    HostServices = servicesForPlugin
                });
        }
    }
}