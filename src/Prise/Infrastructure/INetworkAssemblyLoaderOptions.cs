namespace Prise.Infrastructure
{
    public interface INetworkAssemblyLoaderOptions<T> : IAssemblyLoadOptions<T>
    {
        string BaseUrl { get; }
    }
}