using Prise.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise
{
    public class NetworkAssemblyLoadContext<T> : DefaultAssemblyLoadContext<T>
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl;
        private readonly ITempPathProvider<T> tempPathProvider;

        public NetworkAssemblyLoadContext(
            IPluginLogger<T> logger,
            INetworkAssemblyLoaderOptions<T> options,
            IHostFrameworkProvider hostFrameworkProvider,
            IHostTypesProvider<T> hostTypesProvider,
            IRemoteTypesProvider<T> remoteTypesProvider,
            IDependencyPathProvider<T> dependencyPathProvider,
            IProbingPathsProvider<T> probingPathsProvider,
            IRuntimePlatformContext runtimePlatformContext,
            IDepsFileProvider<T> depsFileProvider,
            IPluginDependencyResolver<T> pluginDependencyResolver,
            INativeAssemblyUnloader nativeAssemblyUnloader,
            IAssemblyLoadStrategyProvider assemblyLoadStrategyProvider,
            IHttpClientFactory httpClientFactory,
            ITempPathProvider<T> tempPathProvider) : base(
                logger,
                options,
                hostFrameworkProvider,
                hostTypesProvider,
                remoteTypesProvider,
                dependencyPathProvider,
                probingPathsProvider,
                runtimePlatformContext,
                depsFileProvider,
                pluginDependencyResolver,
                nativeAssemblyUnloader,
                assemblyLoadStrategyProvider
             )
        {
            this.httpClient = httpClientFactory.CreateClient();
            this.baseUrl = options.BaseUrl;
            this.tempPathProvider = tempPathProvider;
        }

        public override Assembly LoadPluginAssembly(IPluginLoadContext pluginLoadContext)
        {
            this.pluginDependencyContext = PluginDependencyContext.FromPluginAssembly(
                pluginLoadContext,
                this.logger,
                this.hostFrameworkProvider,
                this.hostTypesProvider.ProvideHostTypes(),
                this.remoteTypesProvider.ProvideRemoteTypes(),
                this.runtimePlatformContext,
                this.depsFileProvider,
                this.options.IgnorePlatformInconsistencies);

            using (var pluginStream = LoadPluginFromNetwork(this.baseUrl, pluginLoadContext.PluginAssemblyName))
            {
                this.assemblyLoadStrategy = this.assemblyLoadStrategyProvider.ProvideAssemblyLoadStrategy(this.logger, pluginLoadContext, this.pluginDependencyContext);

                return base.LoadFromStream(pluginStream); // ==> AssemblyLoadContext.LoadFromStream(Stream stream);
            }
        }

        public override async Task<Assembly> LoadPluginAssemblyAsync(IPluginLoadContext pluginLoadContext)
        {
            if (this.pluginDependencyContext != null)
                throw new PrisePluginException($"Plugin {pluginLoadContext.PluginAssemblyName} was already loaded");

            this.pluginDependencyContext = await PluginDependencyContext.FromPluginAssemblyAsync(
                pluginLoadContext,
                this.logger,
                this.hostFrameworkProvider,
                this.hostTypesProvider.ProvideHostTypes(),
                this.remoteTypesProvider.ProvideRemoteTypes(),
                this.runtimePlatformContext,
                this.depsFileProvider,
                this.options.IgnorePlatformInconsistencies);

            using (var pluginStream = await LoadPluginFromNetworkAsync(this.baseUrl, pluginLoadContext.PluginAssemblyName))
            {
                this.assemblyLoadStrategy = this.assemblyLoadStrategyProvider.ProvideAssemblyLoadStrategy(this.logger, pluginLoadContext, this.pluginDependencyContext);

                return base.LoadFromStream(pluginStream); // ==> AssemblyLoadContext.LoadFromStream(Stream stream);
            }
        }

        protected virtual Stream LoadPluginFromNetwork(string baseUrl, string pluginAssemblyName)
        {
            var response = this.httpClient.GetAsync($"{baseUrl}/{pluginAssemblyName}").Result;
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new PrisePluginException($"Remote assembly {pluginAssemblyName} not found at {baseUrl}");

            if (!response.IsSuccessStatusCode)
                throw new PrisePluginException($"Error loading plugin {pluginAssemblyName} at {baseUrl}");

            return response.Content.ReadAsStreamAsync().Result;
        }

        protected async virtual Task<Stream> LoadPluginFromNetworkAsync(string baseUrl, string pluginAssemblyName)
        {
            var response = await this.httpClient.GetAsync($"{baseUrl}/{pluginAssemblyName}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new PrisePluginException($"Remote assembly {pluginAssemblyName} not found at {baseUrl}");

            if (!response.IsSuccessStatusCode)
                throw new PrisePluginException($"Error loading plugin {pluginAssemblyName} at {baseUrl}");

            return await response.Content.ReadAsStreamAsync();
        }

        protected override ValueOrProceed<Assembly> LoadFromRemote(IPluginLoadContext pluginLoadContext, AssemblyName assemblyName)
        {
            var networkAssembly = LoadDependencyFromNetwork(assemblyName);
            if (networkAssembly != null)
                return ValueOrProceed<Assembly>.FromValue(networkAssembly, false);

            return ValueOrProceed<Assembly>.Proceed();
        }

        protected override ValueOrProceed<Assembly> LoadFromDependencyContext(IPluginLoadContext pluginLoadContext, AssemblyName assemblyName)
        {
            if (IsResourceAssembly(assemblyName))
            {
                foreach (var resourceDependency in this.pluginDependencyContext.PluginResourceDependencies)
                {
                    var resourceNetworkPathToAssembly = Path.Combine(this.baseUrl, Path.Combine(resourceDependency.Path, assemblyName.CultureName, $"{assemblyName.Name}.dll"));
                    var resourceNetworkAssembly = LoadDependencyFromNetwork(resourceNetworkPathToAssembly);
                    if (resourceNetworkAssembly != null)
                        return ValueOrProceed<Assembly>.FromValue(resourceNetworkAssembly, false);
                }

                // Do not proceed probing
                return ValueOrProceed<Assembly>.FromValue(null, false);
            }

            var pluginDependency = this.pluginDependencyContext.PluginDependencies.FirstOrDefault(d => d.DependencyNameWithoutExtension == assemblyName.Name);
            if (pluginDependency != null)
            {
                var dependencyPath = this.dependencyPathProvider.GetDependencyPath();
                var probingPaths = this.probingPathsProvider.GetProbingPaths();
                var resolvedNetworkAssembly = this.pluginDependencyResolver.ResolvePluginDependencyToPath(dependencyPath, probingPaths, pluginDependency);
                if (resolvedNetworkAssembly != null)
                    return ValueOrProceed<Assembly>.FromValue(LoadFromStream(resolvedNetworkAssembly), false);
            }

            var networkPathToAssembly = $"{baseUrl}/{Path.Combine(this.dependencyPathProvider.GetDependencyPath(), assemblyName.Name)}.dll";
            var networkAssembly = LoadDependencyFromNetwork(networkPathToAssembly);
            if (networkAssembly != null)
                return ValueOrProceed<Assembly>.FromValue(networkAssembly, false);

            return ValueOrProceed<Assembly>.Proceed();
        }

        protected override ValueOrProceed<string> LoadUnmanagedFromDependencyContext(IPluginLoadContext pluginLoadContext, string unmanagedDllName)
        {
            var unmanagedDllNameWithoutFileExtension = Path.GetFileNameWithoutExtension(unmanagedDllName);
            var platformDependency = this.pluginDependencyContext.PlatformDependencies.FirstOrDefault(d => d.DependencyNameWithoutExtension == unmanagedDllNameWithoutFileExtension);
            if (platformDependency != null)
            {
                var dependencyPath = this.dependencyPathProvider.GetDependencyPath();
                if (String.IsNullOrEmpty(dependencyPath))
                    dependencyPath = pluginLoadContext.PluginAssemblyPath;
                var probingPaths = this.probingPathsProvider.GetProbingPaths();
                var pathToDependency = this.pluginDependencyResolver.ResolvePlatformDependencyToPath(dependencyPath, probingPaths, platformDependency);

                if (!String.IsNullOrEmpty(pathToDependency))
                {
                    string runtimeCandidate = null;
                    if (this.options.NativeDependencyLoadPreference == NativeDependencyLoadPreference.PreferInstalledRuntime)
                        // Prefer loading from runtime folder
                        runtimeCandidate = this.pluginDependencyResolver.ResolvePlatformDependencyPathToRuntime(this.options.PluginPlatformVersion, pathToDependency);

                    return ValueOrProceed<string>.FromValue(runtimeCandidate ?? pathToDependency, false);
                }
            }

            return ValueOrProceed<string>.FromValue(String.Empty, true);
        }

        protected override ValueOrProceed<string> LoadUnmanagedFromRemote(IPluginLoadContext pluginLoadContext, string unmanagedDllName)
        {
            var networkUrl = $"{this.baseUrl}/{unmanagedDllName}";
            var networkFile = NetworkUtil.Download(this.httpClient, networkUrl);
            if (networkFile != null)
                return ValueOrProceed<string>.FromValue(NetworkUtil.SaveToTempFolder(this.tempPathProvider.ProvideTempPath($"{unmanagedDllName}.dll"), networkFile), false);

            return ValueOrProceed<string>.FromValue(String.Empty, true);
        }

        protected virtual Assembly LoadDependencyFromNetwork(AssemblyName assemblyName)
        {
            var pluginAssemblyName = $"{assemblyName.Name}.dll";
            var pathToAssembly = $"{baseUrl}/{pluginAssemblyName}";

            return LoadDependencyFromNetwork(pathToAssembly);
        }

        protected virtual Assembly LoadDependencyFromNetwork(string fullPathToAssembly)
        {
            var dependency = NetworkUtil.DownloadAsStream(this.httpClient, fullPathToAssembly);

            if (dependency == null) return null;

            return base.LoadFromStream(dependency);
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                this.disposing = true;

                GC.Collect();
                GC.WaitForPendingFinalizers();

                this.hostTypesProvider = null;
                this.remoteTypesProvider = null;
                this.dependencyPathProvider = null;
                this.probingPathsProvider = null;
                this.runtimePlatformContext = null;
                this.depsFileProvider = null;
                this.pluginDependencyResolver = null;
                this.pluginDependencyContext = null;
                this.assemblyLoadStrategy = null;

                foreach (var nativeAssembly in this.loadedNativeLibraries)
                    this.nativeAssemblyUnloader.UnloadNativeAssembly(nativeAssembly.Key, nativeAssembly.Value);

                this.loadedNativeLibraries = null;
                this.nativeAssemblyUnloader = null;

                if (this.tempPathProvider != null)
                    this.tempPathProvider.Dispose();
            }
            this.disposed = true;
        }
    }
}
