using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;
using Prise.Infrastructure.NetCore.Contracts;

namespace Prise.Infrastructure.NetCore
{
    public class NetworkAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly string baseUrl;
        private readonly HttpClient httpClient;
        private readonly AssemblyName pluginInfrastructureAssemblyName;

        public NetworkAssemblyLoadContext(string baseUrl, HttpClient httpClient)
        {
            this.baseUrl = baseUrl;
            this.httpClient = httpClient;
            this.pluginInfrastructureAssemblyName = typeof(Prise.Infrastructure.PluginAttribute).Assembly.GetName();
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            if (assemblyName.FullName == this.pluginInfrastructureAssemblyName.FullName)
                return null;

            var deps = DependencyContext.Default;
            var res = deps.CompileLibraries.Where(d => d.Name.Contains(assemblyName.Name)).ToList();
            if (res.Count > 0)
            {
                return Assembly.Load(new AssemblyName(res.First().Name));
            }
            else
            {
                var networkAssembly = LoadDependencyFromNetwork(assemblyName);
                if (networkAssembly != null)
                    return networkAssembly;
            }
            return Assembly.Load(assemblyName);
        }

        protected virtual Assembly LoadDependencyFromNetwork(AssemblyName assemblyName)
        {
            var name = $"{assemblyName.Name}.dll";
            var dependency = DownloadDependency(name);

            if (dependency == null) return null;

            return Assembly.Load(dependency);
        }

        private byte[] DownloadDependency(string pluginAssemblyName)
        {
            var response = this.httpClient.GetAsync($"{baseUrl}/{pluginAssemblyName}").Result;
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            return response.Content.ReadAsByteArrayAsync().Result;
        }
    }

    public class NetworkAssemblyLoader<T> : IPluginAssemblyLoader<T>
    {
        protected readonly IRootPathProvider rootPathProvider;
        protected readonly INetworkAssemblyLoaderOptions options;
        protected readonly HttpClient httpClient;
        protected readonly AssemblyName pluginInfrastructureAssemblyName;

        /// To be used by Dependency Injection
        public NetworkAssemblyLoader(
            IRootPathProvider rootPathProvider,
            INetworkAssemblyLoaderOptions options,
            IHttpClientFactory httpClientFactory) : this(rootPathProvider, options, httpClientFactory.CreateClient()) { }

        internal NetworkAssemblyLoader(
            IRootPathProvider rootPathProvider,
            INetworkAssemblyLoaderOptions options,
            HttpClient httpClient)
        {
            this.options = options;
            this.rootPathProvider = rootPathProvider;
            this.httpClient = httpClient;
            this.pluginInfrastructureAssemblyName = typeof(Prise.Infrastructure.PluginAttribute).Assembly.GetName();
        }

        public async virtual Task<Assembly> Load(string pluginAssemblyName)
        {
            var pluginStream = await LoadPluginFromNetwork(this.options.BaseUrl, pluginAssemblyName);
            var loader = new NetworkAssemblyLoadContext(this.options.BaseUrl, httpClient);
            return loader.LoadFromStream(pluginStream);
        }

        protected async virtual Task<Stream> LoadPluginFromNetwork(string baseUrl, string pluginAssemblyName)
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