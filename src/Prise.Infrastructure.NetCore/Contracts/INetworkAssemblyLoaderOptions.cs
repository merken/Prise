namespace Prise.Infrastructure.NetCore
{
    public interface INetworkAssemblyLoaderOptions : IAssemblyLoadOptions
    {
        string BaseUrl { get; }
    }

    public class NetworkAssemblyLoaderOptions : AssemblyLoadOptions, INetworkAssemblyLoaderOptions
    {
        private readonly string baseUrl;
        public NetworkAssemblyLoaderOptions(string baseUrl,
            PluginPlatformVersion pluginPlatformVersion,
            bool ignorePlatformInconsistencies,
            DependencyLoadPreference dependencyLoadPreference,
            NativeDependencyLoadPreference nativeDependencyLoadPreference
        )
         : base(pluginPlatformVersion, ignorePlatformInconsistencies, dependencyLoadPreference, nativeDependencyLoadPreference)
        {
            this.baseUrl = baseUrl;

        }

        public string BaseUrl => baseUrl;
    }
}