using Prise.Infrastructure;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace Prise
{
    public class NetworkPluginDependencyResolver<T> : DefaultPluginDependencyResolver<T>
    {
        private readonly INetworkAssemblyLoaderOptions<T> options;
        private readonly HttpClient httpClient;
        private readonly ITempPathProvider<T> tempPathProvider;

        public NetworkPluginDependencyResolver(
            IRuntimePlatformContext runtimePlatformContext,
            INetworkAssemblyLoaderOptions<T> options,
            IHttpClientFactory httpClientFactory,
            ITempPathProvider<T> tempPathProvider): base(runtimePlatformContext)
        {
            this.options = options;
            this.httpClient = httpClientFactory.CreateClient();
            this.tempPathProvider = tempPathProvider;
        }

        public override Stream ResolvePluginDependencyToPath(string dependencyPath, IEnumerable<string> probingPaths, PluginDependency dependency)
        {
            var networkFileLocation = $"{this.options.BaseUrl}/{dependency.DependencyPath}";
            var networkFile = NetworkUtil.DownloadAsStream(this.httpClient, networkFileLocation);
            if (networkFile != null)
            {
                return networkFile;
            }

            foreach (var probingPath in probingPaths)
            {
                networkFileLocation = $"{this.options.BaseUrl}/{Path.Combine(probingPath, dependency.ProbingPath)}";
                networkFile = NetworkUtil.DownloadAsStream(this.httpClient, networkFileLocation);
                if (networkFile != null)
                {
                    return networkFile;
                }
            }

            foreach (var candidate in runtimePlatformContext.GetPluginDependencyNames(dependency.DependencyNameWithoutExtension))
            {
                networkFileLocation = $"{this.options.BaseUrl}/{Path.Combine(dependencyPath, candidate)}";
                networkFile = NetworkUtil.DownloadAsStream(this.httpClient, networkFileLocation);
                if (networkFile != null)
                {
                    return networkFile;
                }
            }
            return null;
        }

        public string ResolvePlatformDependencyToPath(string dependencyPath, IEnumerable<string> probingPaths, PlatformDependency dependency)
        {
            foreach (var candidate in runtimePlatformContext.GetPlatformDependencyNames(dependency.DependencyNameWithoutExtension))
            {
                var platformNetworkFileLocation = $"{this.options.BaseUrl}/{candidate}";
                var platformNetworkFile = NetworkUtil.Download(this.httpClient, platformNetworkFileLocation);
                if (platformNetworkFile != null)
                {
                    var localLocation = NetworkUtil.SaveToTempFolder(this.tempPathProvider.ProvideTempPath(Path.GetFileName(candidate)), platformNetworkFile);
                    return localLocation;
                }
            }

            var networkFileLocation = $"{this.options.BaseUrl}/{dependency.DependencyPath}";
            var networkFile = NetworkUtil.Download(this.httpClient, networkFileLocation);
            if (networkFile != null)
            {
                var localLocation = NetworkUtil.SaveToTempFolder(this.tempPathProvider.ProvideTempPath(Path.GetFileName(dependency.DependencyPath)), networkFile);
                return localLocation;
            }

            foreach (var searchPath in probingPaths)
            {
                networkFileLocation = $"{this.options.BaseUrl}/{Path.Combine(searchPath, dependency.ProbingPath)}";
                networkFile = NetworkUtil.Download(this.httpClient, networkFileLocation);
                if (networkFile != null)
                {
                    var localLocation = NetworkUtil.SaveToTempFolder(this.tempPathProvider.ProvideTempPath(Path.GetFileName(dependency.ProbingPath)), networkFile);
                    return localLocation;
                }
            }

            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                if (this.tempPathProvider != null)
                    this.tempPathProvider.Dispose();
            }
            this.disposed = true;
        }
    }
}
