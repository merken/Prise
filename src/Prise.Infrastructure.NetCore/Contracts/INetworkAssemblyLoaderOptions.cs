namespace Prise.Infrastructure.NetCore.Contracts
{
    public interface INetworkAssemblyLoaderOptions
    {
        string BaseUrl { get; }
    }

    public class NetworkAssemblyLoaderOptions : INetworkAssemblyLoaderOptions
    {
        private readonly string baseUrl;
        public NetworkAssemblyLoaderOptions(string baseUrl)
        {
            this.baseUrl = baseUrl;

        }
        public string BaseUrl => baseUrl;
    }
}