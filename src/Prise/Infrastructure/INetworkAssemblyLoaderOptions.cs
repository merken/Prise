namespace Prise.Infrastructure
{
    public interface INetworkAssemblyLoaderOptions : IAssemblyLoadOptions
    {
        string BaseUrl { get; }
    }

    public class NetworkAssemblyLoaderOptions : AssemblyLoadOptions, INetworkAssemblyLoaderOptions
    {
        private readonly string baseUrl;
        public NetworkAssemblyLoaderOptions(string baseUrl,
            PluginPlatformVersion pluginPlatformVersion = null,
            bool ignorePlatformInconsistencies = false,
            DependencyLoadPreference dependencyLoadPreference = DependencyLoadPreference.PreferDependencyContext,
            NativeDependencyLoadPreference nativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferInstalledRuntime
        )
         : base(pluginPlatformVersion, ignorePlatformInconsistencies, dependencyLoadPreference, nativeDependencyLoadPreference)
        {
            this.baseUrl = baseUrl;
        }

        public virtual string BaseUrl => baseUrl;
    }
}