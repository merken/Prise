using Microsoft.Extensions.DependencyInjection;
using System;

namespace Prise.Infrastructure
{
    public interface ISharedServicesProvider<T> : IDisposable
    {
        IServiceCollection ProvideHostServices();
        IServiceCollection ProvideSharedServices();
    }
}