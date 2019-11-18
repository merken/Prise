using Microsoft.Extensions.DependencyInjection;
using System;

namespace Prise.Infrastructure
{
    public interface ISharedServicesProvider : IDisposable
    {
        IServiceCollection ProvideSharedServices();
    }
}