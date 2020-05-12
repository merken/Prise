using Prise.Infrastructure;

namespace Prise
{
    public class DefaultNetworkAssemblyLoaderOptions<T> : DefaultAssemblyLoadOptions<T>, INetworkAssemblyLoaderOptions<T>
    {
        private readonly string baseUrl;
        public DefaultNetworkAssemblyLoaderOptions(string baseUrl,
            PluginPlatformVersion pluginPlatformVersion = null,
            bool ignorePlatformInconsistencies = false,
            NativeDependencyLoadPreference nativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferInstalledRuntime
        )
         : base(pluginPlatformVersion, ignorePlatformInconsistencies, ignorePlatformInconsistencies, nativeDependencyLoadPreference)
        {
            this.baseUrl = baseUrl;
        }

        public virtual string BaseUrl => baseUrl;
    }
}