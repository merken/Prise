using Microsoft.Extensions.DependencyInjection;
using System;

namespace Prise.Infrastructure
{
    public interface ISharedServicesProvider : IDisposable
    {
        IServiceCollection ProvideSharedServices();
    }

    public class DefaultSharedServicesProvider : ISharedServicesProvider
    {
        private readonly IServiceCollection services;
        protected bool disposed = false;

        public DefaultSharedServicesProvider(IServiceCollection services)
        {
            this.services = services;
        }

        public IServiceCollection ProvideSharedServices() => this.services;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                // Nothing to do here
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}