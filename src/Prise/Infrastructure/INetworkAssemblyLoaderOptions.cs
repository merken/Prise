namespace Prise.Infrastructure
{
    public interface INetworkAssemblyLoaderOptions : IAssemblyLoadOptions
    {
        string BaseUrl { get; }
    }

    public class NetworkAssemblyLoaderOptions : DefaultAssemblyLoadOptions, INetworkAssemblyLoaderOptions
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