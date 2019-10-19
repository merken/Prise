using Microsoft.Extensions.DependencyInjection;

namespace Prise.Infrastructure
{
    public interface ISharedServicesProvider
    {
        IServiceCollection ProvideSharedServices();
    }

    public class DefaultSharedServicesProvider : ISharedServicesProvider
    {
        private readonly IServiceCollection services;

        public DefaultSharedServicesProvider(IServiceCollection services)
        {
            this.services = services;
        }

        public IServiceCollection ProvideSharedServices() => this.services;
    }
}