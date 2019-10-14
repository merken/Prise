using System;
using Microsoft.Extensions.DependencyInjection;

namespace Prise.Infrastructure
{
    public interface IPluginBootstrapper
    {
        IServiceCollection Bootstrap(IServiceCollection services);
    }
}