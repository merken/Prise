using Microsoft.Extensions.DependencyInjection;

namespace Prise.Plugin
{
    public interface IPluginBootstrapper 
    {
        IServiceCollection Bootstrap(IServiceCollection services);
    }
}