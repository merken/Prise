namespace Prise.Infrastructure
{
    public interface INetworkAssemblyLoaderOptions<T> : IAssemblyLoadOptions<T>
    {
        string BaseUrl { get; }
    }

    public class NetworkAssemblyLoaderOptions<T> : DefaultAssemblyLoadOptions<T>, INetworkAssemblyLoaderOptions<T>
    {
        private readonly string baseUrl;
        public NetworkAssemblyLoaderOptions(string baseUrl,
            PluginPlatformVersion pluginPlatformVersion = null,
            bool ignorePlatformInconsistencies = false,
            NativeDependencyLoadPreference nativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferInstalledRuntime
        )
         : base(pluginPlatformVersion, ignorePlatformInconsistencies, nativeDependencyLoadPreference)
        {
            this.baseUrl = baseUrl;
        }

        public virtual string BaseUrl => baseUrl;
    }
}