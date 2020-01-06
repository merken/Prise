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

            assemblies = pluginLoadOptions.AssemblySelector.SelectAssemblies(assemblies);
            if (!assemblies.Any())
                throw new PrisePluginException($@"AssemblySelector returned no assemblies. Requested plugin type: {typeof(T).Name}. Please add the {nameof(PluginAttribute)} to your plugin class and specify the PluginType: [Plugin(PluginType = typeof({typeof(T).Name}))]");

            foreach (var loadContext in assemblies.Select(a => DefaultPluginLoadContext<T>.FromAssemblyScanResult(a)))
            {
                var pluginAssembly = pluginLoadOptions.AssemblyLoader.Load(loadContext);
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
            pluginLoadOptions.AssemblyLoader.Unload();
        }

        protected async Task UnloadAsync<T>(IPluginLoadOptions<T> pluginLoadOptions)
        {
            await pluginLoadOptions.AssemblyLoader.UnloadAsync();
        }

        protected T[] CreatePluginInstances<T>(IPluginLoadOptions<T> pluginLoadOptions, ref Assembly pluginAssembly)
        {
            var pluginInstances = new List<T>();
            var pluginTypes = pluginAssembly
                            .GetTypes()
                            .Where(t => t.CustomAttributes
                                .Any(c => c.AttributeType.Name == typeof(Prise.Plugin.PluginAttribute).Name
                                && (c.NamedArguments.First(a => a.MemberName == "PluginType").TypedValue.Value as Type).Name == typeof(T).Name))
                            .OrderBy(t => t.Name)
                            .AsEnumerable();

            if (pluginTypes == null || !pluginTypes.Any())
                throw new PrisePluginException($@"No plugin was found in assembly {pluginAssembly.FullName}. Requested plugin type: {typeof(T).Name}. Please add the {nameof(PluginAttribute)} to your plugin class and specify the PluginType: [Plugin(PluginType = typeof({typeof(T).Name}))]");

            pluginTypes = pluginLoadOptions.PluginSelector.SelectPlugins(pluginTypes);

            if (!pluginTypes.Any())
                throw new PrisePluginException($@"Selector returned no plugin for {pluginAssembly.FullName}. Requested plugin type: {typeof(T).Name}. Please add the {nameof(PluginAttribute)} to your plugin class and specify the PluginType: [Plugin(PluginType = typeof({typeof(T).Name}))]");

            foreach (var pluginType in pluginTypes)
            {
                var bootstrapperType = GetPluginBootstrapper(ref pluginAssembly, pluginType);
                var pluginFactoryMethod = GetPluginFactoryMethod(pluginType);

                IPluginBootstrapper bootstrapper = null;
                if (bootstrapperType != null)
                {
                    var remoteBootstrapperInstance = pluginLoadOptions.Activator.CreateRemoteBootstrapper(bootstrapperType, pluginAssembly);
                    var remoteBootstrapperProxy = pluginLoadOptions.ProxyCreator.CreateBootstrapperProxy(remoteBootstrapperInstance);
                    this.disposables.Add(remoteBootstrapperProxy as IDisposable);
                    bootstrapper = remoteBootstrapperProxy;
                }

                var remoteObject = pluginLoadOptions.Activator.CreateRemoteInstance(pluginType, bootstrapper, pluginFactoryMethod, pluginAssembly);
                var remoteProxy = pluginLoadOptions.ProxyCreator.CreatePluginProxy(remoteObject, pluginLoadOptions);
                this.disposables.Add(remoteProxy as IDisposable);
                pluginInstances.Add(remoteProxy);
            }

            return pluginInstances.ToArray();
        }

        protected Type GetPluginBootstrapper(ref Assembly pluginAssembly, Type pluginType)
        {
            return pluginAssembly
                    .GetTypes()
                    .Where(t => t.CustomAttributes
                        .Any(c => c.AttributeType.Name == typeof(Prise.Plugin.PluginBootstrapperAttribute).Name &&
                        (c.NamedArguments.First(a => a.MemberName == "PluginType").TypedValue.Value as Type).Name == pluginType.Name)).FirstOrDefault();
        }

        protected MethodInfo GetPluginFactoryMethod(Type pluginType)
        {
            return pluginType.GetMethods()
                    .Where(m => m.CustomAttributes
                        .Any(c => c.AttributeType.Name == typeof(Prise.Plugin.PluginFactoryAttribute).Name)).FirstOrDefault();
        }
    }
}