using System;
using System.Reflection;
using Prise.Infrastructure.NetCore.Contracts;

namespace Prise.Infrastructure.NetCore
{
    public interface IPluginLoadOptions<T>
    {
        IRootPathProvider RootPathProvider { get; }
        IRemotePluginActivator Activator { get; }
        IResultConverter ResultConverter { get; }
        IParameterConverter ParameterConverter { get; }
        IPluginAssemblyLoader<T> AssemblyLoader { get; }
        string PluginAssemblyName { get; }
        bool SupportMultiplePlugins { get; }
    }

    public class PluginLoadOptions<T> : IPluginLoadOptions<T>
    {
        private readonly IRootPathProvider rootPathProvider;
        private readonly IRemotePluginActivator activator;
        private readonly IResultConverter resultConverter;
        private readonly IParameterConverter parameterConverter;
        private readonly IPluginAssemblyLoader<T> assemblyLoader;
        private readonly string pluginAssemblyName;
        private readonly bool supportMultiplePlugins;

        public PluginLoadOptions(
            IRootPathProvider rootPathProvider,
            IRemotePluginActivator activator,
            IParameterConverter parameterConverter,
            IResultConverter resultConverter,
            IPluginAssemblyLoader<T> assemblyLoader,
            string pluginAssemblyName,
            bool supportMultiplePlugins = false)
        {
            this.rootPathProvider = rootPathProvider;
            this.activator = activator;
            this.parameterConverter = parameterConverter;
            this.resultConverter = resultConverter;
            this.assemblyLoader = assemblyLoader;
            this.pluginAssemblyName = pluginAssemblyName;
            this.supportMultiplePlugins = supportMultiplePlugins;
        }

        public IRootPathProvider RootPathProvider => this.rootPathProvider;
        public IRemotePluginActivator Activator => this.activator;
        public IResultConverter ResultConverter => this.resultConverter;
        public IParameterConverter ParameterConverter => this.parameterConverter;
        public IPluginAssemblyLoader<T> AssemblyLoader => this.assemblyLoader;
        public string PluginAssemblyName => this.pluginAssemblyName;
        public bool SupportMultiplePlugins => this.supportMultiplePlugins;
    }

    public class PluggerOptionsBuilder<T>
    {
        internal IRootPathProvider rootPathProvider;
        internal IRemotePluginActivator activator;
        internal IResultConverter resultConverter;
        internal IParameterConverter parameterConverter;
        internal IPluginAssemblyLoader<T> assemblyLoader;
        internal string pluginAssemblyName;
        internal bool supportMultiplePlugins;

        internal PluggerOptionsBuilder()
        {
        }

        public PluggerOptionsBuilder<T> WithRootPath(string path)
        {
            this.rootPathProvider = new RootPathProvider(path);
            return this;
        }

        public PluggerOptionsBuilder<T> WithActivator(IRemotePluginActivator activator)
        {
            this.activator = activator;
            return this;
        }

        public PluggerOptionsBuilder<T> WithParameterConverter(IParameterConverter parameterConverter)
        {
            this.parameterConverter = parameterConverter;
            return this;
        }

        public PluggerOptionsBuilder<T> WithResultConverter(IResultConverter resultConverter)
        {
            this.resultConverter = resultConverter;
            return this;
        }

        public PluggerOptionsBuilder<T> WithAssemblyLoader(IPluginAssemblyLoader<T> assemblyLoader)
        {
            this.assemblyLoader = assemblyLoader;
            return this;
        }

        public PluggerOptionsBuilder<T> WithLocalDiskAssemblyLoader(LocalAssemblyLoaderOptions options)
        {
            this.assemblyLoader = new LocalDiskAssemblyLoader<T>(this.rootPathProvider, options);
            return this;
        }

        public PluggerOptionsBuilder<T> SupportMultiplePlugins(bool supportMultiplePlugins = true)
        {
            this.supportMultiplePlugins = supportMultiplePlugins;
            return this;
        }

        public PluggerOptionsBuilder<T> WithPluginAssemblyName(string pluginAssemblyName)
        {
            this.pluginAssemblyName = pluginAssemblyName;
            return this;
        }

        public PluggerOptionsBuilder<T> WithDefaultOptions(string rootPath = null)
        {
            if (String.IsNullOrEmpty(rootPath))
                rootPath = GetLocalExecutionPath();

            this.rootPathProvider = new RootPathProvider(rootPath);
            this.activator = new NetCoreActivator();
            this.parameterConverter = new NewtonsoftParameterConverter();
            this.resultConverter = new BinaryFormatterResultConverter();
            this.assemblyLoader = new LocalDiskAssemblyLoader<T>(this.rootPathProvider, new LocalAssemblyLoaderOptions("Plugins"));
            this.pluginAssemblyName = $"{typeof(T).Name}.dll";

            return this;
        }

        internal IPluginLoadOptions<T> Build()
        {
            return new PluginLoadOptions<T>(
                this.rootPathProvider,
                 this.activator,
                 this.parameterConverter,
                 this.resultConverter,
                 this.assemblyLoader,
                 this.pluginAssemblyName,
                 this.supportMultiplePlugins);
        }

        private string GetLocalExecutionPath()
        {
            var localExecutionPath = Assembly.GetExecutingAssembly().Location;
            var paths = localExecutionPath.Split("\\");
            return String.Join("\\", paths, 0, paths.Length - 1);
        }
    }
}