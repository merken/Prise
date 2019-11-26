using Prise.Infrastructure;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Prise
{
    public class NetworkDepsFileProvider<T> : IDepsFileProvider<T>
    {
        private readonly INetworkAssemblyLoaderOptions<T> options;
        private readonly HttpClient client;
        private Stream stream;
        private bool disposed = false;

        public NetworkDepsFileProvider(
            INetworkAssemblyLoaderOptions<T> options, 
            IHttpClientFactory httpClientFactory)
        {
            this.options = options;
            this.client = httpClientFactory.CreateClient();
        }

        public virtual async Task<Stream> ProvideDepsFile(IPluginLoadContext pluginLoadContext)
        {
            var url = $"{this.options.BaseUrl}/{Path.GetFileNameWithoutExtension(pluginLoadContext.PluginAssemblyName)}.deps.json";
            var response = await this.client.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new PrisePluginException($"Dependency file (deps file) not found at {url}");

            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new PrisePluginException($"Dependency file (deps file) load error at {url} {content}");

            this.stream = CreateStreamFromString(content);
            return this.stream;
        }

        public static Stream CreateStreamFromString(string content)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                this.stream?.Dispose();
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
