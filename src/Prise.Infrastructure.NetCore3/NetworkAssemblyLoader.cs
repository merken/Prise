using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Prise.Infrastructure.NetCore.Contracts;

namespace Prise.Infrastructure.NetCore3
{
    public class NetworkAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly string rootPath;
        private readonly string baseUrl;
        private readonly HttpClient httpClient;
        private readonly AssemblyName pluginInfrastructureAssemblyName;
        private AssemblyDependencyResolver resolver;

        public NetworkAssemblyLoadContext(string rootPath, string baseUrl, HttpClient httpClient)
        {
            this.rootPath = rootPath;
            this.baseUrl = baseUrl;
            this.httpClient = httpClient;
            this.pluginInfrastructureAssemblyName = typeof(Prise.Infrastructure.PluginAttribute).Assembly.GetName();
            this.resolver = new AssemblyDependencyResolver(rootPath);
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

        internal byte[] DownloadDependency(string pluginAssemblyName)
        {
            var response = this.httpClient.GetAsync($"{baseUrl}/{pluginAssemblyName}").Result;
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            return response.Content.ReadAsByteArrayAsync().Result;
        }
    }

    public class NetworkAssemblyLoader<T> : IPluginAssemblyLoader<T>
    {
        private readonly IRootPathProvider rootPathProvider;
        private readonly INetworkAssemblyLoaderOptions options;
        private readonly HttpClient httpClient;
        private readonly AssemblyName pluginInfrastructureAssemblyName;

        public NetworkAssemblyLoader(
            IRootPathProvider rootPathProvider,
            INetworkAssemblyLoaderOptions options,
            HttpClient httpClient)
        {
            this.options = options;
            this.rootPathProvider = rootPathProvider;
            this.httpClient = httpClient;
            this.pluginInfrastructureAssemblyName = typeof(Prise.Infrastructure.PluginAttribute).Assembly.GetName();
        }

        public async Task<Assembly> Load(string pluginAssemblyName)
        {
            var pluginStream = await LoadPluginFromNetwork(this.options.BaseUrl, pluginAssemblyName);
            var loader = new NetworkAssemblyLoadContext(this.rootPathProvider.GetRootPath(), this.options.BaseUrl, this.httpClient);
            return loader.LoadFromStream(pluginStream);
        }

        private async Task<Stream> LoadPluginFromNetwork(string baseUrl, string pluginAssemblyName)
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
