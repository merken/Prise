using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Prise.Infrastructure;

namespace Prise
{
    public class DefaultSharedServicesProvider<T> : ISharedServicesProvider<T>
    {
        private readonly IServiceCollection hostServices;
        private readonly IServiceCollection sharedServices;
        protected bool disposed = false;

        public DefaultSharedServicesProvider(IServiceCollection hostServices, IServiceCollection sharedServices)
        {
            this.hostServices = hostServices;
            this.sharedServices = sharedServices;
        }

        public IServiceCollection ProvideHostServices() => this.hostServices;

        public IServiceCollection ProvideSharedServices() => this.sharedServices;

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
