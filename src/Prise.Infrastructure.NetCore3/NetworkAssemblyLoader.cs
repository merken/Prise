using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Prise.Infrastructure.NetCore.Contracts;

namespace Prise.Infrastructure.NetCore
{
    public class NetworkAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly HttpClient httpClient;
        private readonly AssemblyName pluginInfrastructureAssemblyName;
        private readonly AssemblyDependencyResolver resolver;
        private string baseUrl;
        private bool isConfigured;

        public NetworkAssemblyLoadContext(string rootPath, HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.pluginInfrastructureAssemblyName = typeof(Prise.Infrastructure.PluginAttribute).Assembly.GetName();
            this.resolver = new AssemblyDependencyResolver(rootPath);
        }

        internal void Configure(string baseUrl)
        {
            if (this.isConfigured)
                throw new NotSupportedException($"This NetworkAssemblyLoadContext is already configured for {this.baseUrl}. Could not configure for {baseUrl}");

            this.baseUrl = baseUrl;

            this.isConfigured = true;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            if (assemblyName.FullName == this.pluginInfrastructureAssemblyName.FullName)
                return null;

            string assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return LoadDependencyFromNetwork(assemblyName);
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }

        protected virtual Assembly LoadDependencyFromNetwork(AssemblyName assemblyName)
        {
            var name = $"{assemblyName.Name}.dll";
            var dependency = DownloadDependency(name);

            if (dependency == null) return null;

            return Assembly.Load(dependency);
        }

        protected byte[] DownloadDependency(string pluginAssemblyName)
        {
            var response = this.httpClient.GetAsync($"{baseUrl}/{pluginAssemblyName}").Result;
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            return response.Content.ReadAsByteArrayAsync().Result;
        }
    }

    public class NetworkAssemblyLoader<T> : DisposableAssemblyUnLoader, IPluginAssemblyLoader<T>
    {
        private readonly INetworkAssemblyLoaderOptions options;
        private readonly HttpClient httpClient;
        internal NetworkAssemblyLoadContext context;

        public NetworkAssemblyLoader(
            IRootPathProvider rootPathProvider,
            INetworkAssemblyLoaderOptions options,
            HttpClient httpClient)
        {
            this.options = options;
            this.httpClient = httpClient;
            this.context = new NetworkAssemblyLoadContext(rootPathProvider.GetRootPath(), httpClient);
        }

        public virtual Assembly Load(string pluginAssemblyName)
        {
            var pluginStream = LoadPluginFromNetwork(this.options.BaseUrl, pluginAssemblyName);
            this.context.Configure(this.options.BaseUrl);
            return this.context.LoadFromStream(pluginStream);
        }

        public async virtual Task<Assembly> LoadAsync(string pluginAssemblyName)
        {
            var pluginStream = await LoadPluginFromNetworkAsync(this.options.BaseUrl, pluginAssemblyName);
            this.context.Configure(this.options.BaseUrl);
            return this.context.LoadFromStream(pluginStream);
        }

        protected virtual Stream LoadPluginFromNetwork(string baseUrl, string pluginAssemblyName)
        {
            var response = this.httpClient.GetAsync($"{baseUrl}/{pluginAssemblyName}").Result;
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new FileNotFoundException($"Remote assembly {pluginAssemblyName} not found at {baseUrl}");

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Error loading plugin {pluginAssemblyName} at {baseUrl}");

            return response.Content.ReadAsStreamAsync().Result;
        }

        protected async virtual Task<Stream> LoadPluginFromNetworkAsync(string baseUrl, string pluginAssemblyName)
        {
            var response = await this.httpClient.GetAsync($"{baseUrl}/{pluginAssemblyName}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new FileNotFoundException($"Remote assembly {pluginAssemblyName} not found at {baseUrl}");

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Error loading plugin {pluginAssemblyName} at {baseUrl}");

            return await response.Content.ReadAsStreamAsync();
        }
    }
}
