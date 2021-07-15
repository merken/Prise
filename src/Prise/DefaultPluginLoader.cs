using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Prise.Activation;
using Prise.AssemblyLoading;
using Prise.AssemblyScanning;
using Prise.Proxy;
using Prise.Utils;

namespace Prise
{
    public class DefaultPluginLoader : IPluginLoader
    {
        private readonly IAssemblyScanner assemblyScanner;
        private readonly IPluginTypeSelector pluginTypeSelector;
        private readonly IAssemblyLoader assemblyLoader;
        private readonly IParameterConverter parameterConverter;
        private readonly IResultConverter resultConverter;
        private readonly IPluginActivator pluginActivator;
        protected readonly ConcurrentBag<IPluginLoadContext> pluginContexts;
        public DefaultPluginLoader(
                            IAssemblyScanner assemblyScanner,
                            IPluginTypeSelector pluginTypeSelector,
                            IAssemblyLoader assemblyLoader,
                            IParameterConverter parameterConverter,
                            IResultConverter resultConverter,
                            IPluginActivator pluginActivator)
        {
            this.assemblyScanner = assemblyScanner;
            this.pluginTypeSelector = pluginTypeSelector;
            this.assemblyLoader = assemblyLoader;
            this.parameterConverter = parameterConverter;
            this.resultConverter = resultConverter;
            this.pluginActivator = pluginActivator;
            this.pluginContexts = new ConcurrentBag<IPluginLoadContext>();
        }

        public async Task<AssemblyScanResult> FindPlugin<T>(string pathToPlugin)
        {
            return
                (await this.FindPlugins<T>(pathToPlugin))
                .FirstOrDefault();
        }

        public async Task<AssemblyScanResult> FindPlugin<T>(string pathToPlugins, string plugin)
        {
            return
                (await this.FindPlugins<T>(pathToPlugins))
                .FirstOrDefault(p => p.AssemblyPath.Split(Path.DirectorySeparatorChar).Last().Equals(plugin));
        }

        public Task<IEnumerable<AssemblyScanResult>> FindPlugins<T>(string pathToPlugins)
        {
            return this.assemblyScanner.Scan(new AssemblyScannerOptions
            {
                StartingPath = pathToPlugins,
                PluginType = typeof(T)
            });
        }

        public async Task<T> LoadPlugin<T>(AssemblyScanResult scanResult, string hostFramework = null, Action<PluginLoadContext> configureLoadContext = null)
        {
            hostFramework = hostFramework.ValueOrDefault(HostFrameworkUtils.GetHostframeworkFromHost());

            var pathToAssembly = Path.Combine(scanResult.AssemblyPath, scanResult.AssemblyName);
            var pluginLoadContext = PluginLoadContext.DefaultPluginLoadContext(pathToAssembly, typeof(T), hostFramework);
            // This allows the loading of netstandard plugins
            pluginLoadContext.IgnorePlatformInconsistencies = true;

            configureLoadContext?.Invoke(pluginLoadContext);

            this.pluginContexts.Add(pluginLoadContext);

            var pluginAssembly = await this.assemblyLoader.Load(pluginLoadContext);
            var pluginTypes = this.pluginTypeSelector.SelectPluginTypes<T>(pluginAssembly);
            var pluginType = pluginTypes.FirstOrDefault(p => p.Name.Equals(scanResult.PluginType.Name));
            if (pluginType == null)
                throw new PluginLoadException($"Did not found any plugins to load from {nameof(AssemblyScanResult)} {scanResult.AssemblyPath} {scanResult.AssemblyName}");

            return await this.pluginActivator.ActivatePlugin<T>(new DefaultPluginActivationOptions
            {
                PluginType = pluginType,
                PluginAssembly = pluginAssembly,
                ParameterConverter = this.parameterConverter,
                ResultConverter = this.resultConverter,
                HostServices = pluginLoadContext.HostServices
            });
        }

        public async Task<IEnumerable<T>> LoadPlugins<T>(AssemblyScanResult scanResult, string hostFramework = null, Action<PluginLoadContext> configureLoadContext = null)
        {
            var plugins = new List<T>();

#if SUPPORTS_ASYNC_STREAMS
            await foreach (var plugin in this.LoadPluginsAsAsyncEnumerable<T>(scanResult, hostFramework, configureLoadContext))
#else
            foreach (var plugin in await this.LoadPluginsAsAsyncEnumerable<T>(scanResult, hostFramework, configureLoadContext))
#endif
                plugins.Add(plugin);

            return plugins;
        }

#if SUPPORTS_ASYNC_STREAMS
        public async IAsyncEnumerable<T> LoadPluginsAsAsyncEnumerable<T>(AssemblyScanResult scanResult, string hostFramework = null, Action<PluginLoadContext> configureLoadContext = null)
        {
#else
        public async Task<IEnumerable<T>> LoadPluginsAsAsyncEnumerable<T>(AssemblyScanResult scanResult, string hostFramework = null, Action<PluginLoadContext> configureLoadContext = null)
        {
#endif
            hostFramework = hostFramework.ValueOrDefault(HostFrameworkUtils.GetHostframeworkFromHost());

            var pathToAssembly = Path.Combine(scanResult.AssemblyPath, scanResult.AssemblyName);
            var pluginLoadContext = PluginLoadContext.DefaultPluginLoadContext(pathToAssembly, typeof(T), hostFramework);
            // This allows the loading of netstandard plugins
            pluginLoadContext.IgnorePlatformInconsistencies = true;

            configureLoadContext?.Invoke(pluginLoadContext);

            var pluginAssembly = await this.assemblyLoader.Load(pluginLoadContext);

            this.pluginContexts.Add(pluginLoadContext);

            var pluginTypes = this.pluginTypeSelector.SelectPluginTypes<T>(pluginAssembly);

#if SUPPORTS_ASYNC_STREAMS
            foreach (var pluginType in pluginTypes)
                yield return await this.pluginActivator.ActivatePlugin<T>(new DefaultPluginActivationOptions
                {
                    PluginType = pluginType,
                    PluginAssembly = pluginAssembly,
                    ParameterConverter = this.parameterConverter,
                    ResultConverter = this.resultConverter,
                    HostServices = pluginLoadContext.HostServices
                });
#else
            var plugins = new List<T>();
            foreach (var pluginType in pluginTypes)
                plugins.Add(await this.pluginActivator.ActivatePlugin<T>(new DefaultPluginActivationOptions
                {
                    PluginType = pluginType,
                    PluginAssembly = pluginAssembly,
                    ParameterConverter = this.parameterConverter,
                    ResultConverter = this.resultConverter,
                    HostServices = pluginLoadContext.HostServices
                }));
            return plugins;
#endif
        }

        public void UnloadAll()
        {
            foreach (IPluginLoadContext context in pluginContexts)
            {
                assemblyLoader?.Unload(context);
            }

            pluginContexts?.Clear();
        }

        protected volatile bool disposed;

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            UnloadAll();
        }
    }
}