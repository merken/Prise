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
            DependencyLoadPreference dependencyLoadPreference,
            NativeDependencyLoadPreference nativeDependencyLoadPreference
        )
         : base(pluginPlatformVersion, dependencyLoadPreference, nativeDependencyLoadPreference)
        {
            this.baseUrl = baseUrl;

        }

        public string BaseUrl => baseUrl;
    }
}