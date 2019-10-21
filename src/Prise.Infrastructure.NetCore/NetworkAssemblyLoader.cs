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
        protected readonly HttpClient httpClient;
        protected readonly AssemblyName pluginInfrastructureAssemblyName;
        protected string baseUrl;
        protected bool isConfigured;

        public NetworkAssemblyLoadContext(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.pluginInfrastructureAssemblyName = typeof(Prise.Infrastructure.PluginAttribute).Assembly.GetName();
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

        protected byte[] DownloadDependency(string pluginAssemblyName)
        {
            var response = this.httpClient.GetAsync($"{baseUrl}/{pluginAssemblyName}").Result;
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            return response.Content.ReadAsByteArrayAsync().Result;
        }
    }

    public class NetworkAssemblyLoader<T> : IPluginAssemblyLoader<T>
    {
        protected readonly INetworkAssemblyLoaderOptions options;
        protected readonly AssemblyName pluginInfrastructureAssemblyName;
        protected readonly HttpClient httpClient;
        protected NetworkAssemblyLoadContext context;
        protected bool disposed = false;

        /// To be used by Dependency Injection
        public NetworkAssemblyLoader(
            INetworkAssemblyLoaderOptions options,
            IHttpClientFactory httpClientFactory) : this(options, httpClientFactory.CreateClient()) { }

        internal NetworkAssemblyLoader(
            INetworkAssemblyLoaderOptions options,
            HttpClient httpClient)
        {
            this.options = options;
            this.httpClient = httpClient;
            this.context = new NetworkAssemblyLoadContext(httpClient);
            this.pluginInfrastructureAssemblyName = typeof(Prise.Infrastructure.PluginAttribute).Assembly.GetName();
        }

        public async virtual Task<Assembly> Load(string pluginAssemblyName)
        {
            var pluginStream = await LoadPluginFromNetwork(this.options.BaseUrl, pluginAssemblyName);
            this.context.Configure(this.options.BaseUrl);
            return this.context.LoadFromStream(pluginStream);
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

        public async virtual Task Unload()
        {
            GC.Collect(); // collects all unused memory
            GC.WaitForPendingFinalizers(); // wait until GC has finished its work
            GC.Collect();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                GC.Collect(); // collects all unused memory
                GC.WaitForPendingFinalizers(); // wait until GC has finished its work
                GC.Collect();
            }
            this.disposed = true;
        }
    }
}