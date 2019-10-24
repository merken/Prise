using Prise.Infrastructure;

namespace Prise.Infrastructure.NetCore.Contracts
{
    public interface INetworkAssemblyLoaderOptions : IAssemblyLoadOptions
    {
        string BaseUrl { get; }
    }

    public class NetworkAssemblyLoaderOptions : AssemblyLoadOptions, INetworkAssemblyLoaderOptions
    {
        private readonly string baseUrl;
        public NetworkAssemblyLoaderOptions(string baseUrl, DependencyLoadPreference dependencyLoadPreference = DependencyLoadPreference.PreferRemote)
         : base(dependencyLoadPreference)
        {
            this.baseUrl = baseUrl;

        }

        public string BaseUrl => baseUrl;
    }
}