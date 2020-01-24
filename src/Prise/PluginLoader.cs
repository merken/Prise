using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Prise.Infrastructure;
using Prise.Plugin;

namespace Prise
{
    public abstract class PluginLoader
    {
        protected List<Assembly> pluginAssemblies;
        protected List<IDisposable> disposables;

        protected PluginLoader()
        {
            this.pluginAssemblies = new List<Assembly>();
            this.disposables = new List<IDisposable>();
        }

        protected T[] LoadPluginsOfType<T>(IPluginLoadOptions<T> pluginLoadOptions)
        {
            var instances = new List<T>();
            var assemblies = pluginLoadOptions.AssemblyScanner.Scan().Result;

            if (!assemblies.Any())
                throw new PrisePluginException($"No plugins of type {typeof(T).Name} found after scanning assemblies.");

            foreach (var result in assemblies)
                pluginLoadOptions.Logger.PluginAssemblyDiscovered(result);

            assemblies = pluginLoadOptions.AssemblySelector.SelectAssemblies(assemblies);
            if (!assemblies.Any())
                throw new PrisePluginException($@"AssemblySelector returned no assemblies. Requested plugin type: {typeof(T).Name}. Please add the {nameof(PluginAttribute)} to your plugin class and specify the PluginType: [Plugin(PluginType = typeof({typeof(T).Name}))]");

            foreach (var result in assemblies)
                pluginLoadOptions.Logger.PluginAssemblySelected(result);

            foreach (var loadContext in assemblies.Select(a => DefaultPluginLoadContext<T>.FromAssemblyScanResult(a)))
            {
                pluginLoadOptions.Logger.PluginContextCreated(loadContext);

                var pluginAssembly = pluginLoadOptions.AssemblyLoader.Load(loadContext);
                pluginLoadOptions.Logger.PluginLoaded(pluginAssembly);

                this.pluginAssemblies.Add(pluginAssembly);
                var pluginInstances = CreatePluginInstances(pluginLoadOptions, ref pluginAssembly);
                instances.AddRange(pluginInstances);
            }

            if (!instances.Any())
                throw new PrisePluginException($"No plugins of type {typeof(T).Name} found from assemblies {String.Join(',', assemblies.Select(a => a.AssemblyPath))}");

            return instances.ToArray();
        }

        protected async Task<T[]> LoadPluginsOfTypeAsync<T>(IPluginLoadOptions<T> pluginLoadOptions)
        {
            var instances = new List<T>();
            var assemblies = await pluginLoadOptions.AssemblyScanner.Scan();

            if (!assemblies.Any())
                throw new PrisePluginException($"No plugins of type {typeof(T).Name} found after scanning assemblies.");

            assemblies = pluginLoadOptions.AssemblySelector.SelectAssemblies(assemblies);
            if (!assemblies.Any())
                throw new PrisePluginException($@"AssemblySelector returned no assemblies. Requested plugin type: {typeof(T).Name}. Please add the {nameof(PluginAttribute)} to your plugin class and specify the PluginType: [Plugin(PluginType = typeof({typeof(T).Name}))]");

            foreach (var loadContext in assemblies.Select(a => DefaultPluginLoadContext<T>.FromAssemblyScanResult(a)))
            {
                var pluginAssembly = await pluginLoadOptions.AssemblyLoader.LoadAsync(loadContext);
                this.pluginAssemblies.Add(pluginAssembly);
                var pluginInstances = CreatePluginInstances(pluginLoadOptions, ref pluginAssembly);
                instances.AddRange(pluginInstances);
            }

            if (!instances.Any())
                throw new PrisePluginException($"No plugins of type {typeof(T).Name} found from assemblies {String.Join(',', assemblies.Select(a => a.AssemblyPath))}");

            return instances.ToArray();
        }

        protected void Unload<T>(IPluginLoadOptions<T> pluginLoadOptions)
        {
            pluginLoadOptions.AssemblyLoader.UnloadAll();
        }

        protected async Task UnloadAsync<T>(IPluginLoadOptions<T> pluginLoadOptions)
        {
            await pluginLoadOptions.AssemblyLoader.UnloadAllAsync();
        }

        protected T[] CreatePluginInstances<T>(IPluginLoadOptions<T> pluginLoadOptions, ref Assembly pluginAssembly)
        {
            var pluginInstances = new List<T>();
            var pluginTypes = pluginLoadOptions.PluginTypesProvider.ProvidePluginTypes(pluginAssembly);

            if (pluginTypes == null || !pluginTypes.Any())
                throw new PrisePluginException($@"No plugin was found in assembly {pluginAssembly.FullName}. Requested plugin type: {typeof(T).Name}. Please add the {nameof(PluginAttribute)} to your plugin class and specify the PluginType: [Plugin(PluginType = typeof({typeof(T).Name}))]");

            foreach (var type in pluginTypes)
                pluginLoadOptions.Logger.PluginTypeProvided(type);

            pluginTypes = pluginLoadOptions.PluginSelector.SelectPlugins(pluginTypes);

            if (!pluginTypes.Any())
                throw new PrisePluginException($@"Selector returned no plugin for {pluginAssembly.FullName}. Requested plugin type: {typeof(T).Name}. Please add the {nameof(PluginAttribute)} to your plugin class and specify the PluginType: [Plugin(PluginType = typeof({typeof(T).Name}))]");

            foreach (var type in pluginTypes)
                pluginLoadOptions.Logger.PluginTypeSelected(type);

            foreach (var pluginType in pluginTypes)
            {
                T pluginProxy = default(T);
                IPluginBootstrapper bootstrapperProxy = null;

                var pluginActivationContext = pluginLoadOptions.PluginActivationContextProvider.ProvideActivationContext(pluginType, pluginAssembly);

                pluginLoadOptions.Logger.PluginActivationContextProvided(pluginActivationContext);

                if (pluginActivationContext.PluginBootstrapperType != null)
                {
                    var remoteBootstrapperInstance = pluginLoadOptions.Activator.CreateRemoteBootstrapper(pluginActivationContext.PluginBootstrapperType, pluginAssembly);
                    pluginLoadOptions.Logger.RemoteBootstrapperActivated(remoteBootstrapperInstance);

                    var remoteBootstrapperProxy = pluginLoadOptions.ProxyCreator.CreateBootstrapperProxy(remoteBootstrapperInstance);
                    pluginLoadOptions.Logger.RemoteBootstrapperProxyCreated(remoteBootstrapperProxy);

                    this.disposables.Add(remoteBootstrapperProxy as IDisposable);
                    bootstrapperProxy = remoteBootstrapperProxy;
                }

                var remoteObject = pluginLoadOptions.Activator.CreateRemoteInstance(
                    pluginActivationContext,
                    bootstrapperProxy
                );

                pluginLoadOptions.Logger.RemoteInstanceCreated(remoteObject);

                pluginProxy = pluginLoadOptions.ProxyCreator.CreatePluginProxy(remoteObject, pluginLoadOptions.ParameterConverter, pluginLoadOptions.ResultConverter);
                pluginLoadOptions.Logger.RemoteProxyCreated(pluginProxy);

                this.disposables.Add(pluginProxy as IDisposable);
                pluginInstances.Add(pluginProxy);
            }

            return pluginInstances.ToArray();
        }
    }
}