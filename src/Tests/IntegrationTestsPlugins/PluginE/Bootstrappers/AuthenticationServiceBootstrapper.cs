using Microsoft.Extensions.DependencyInjection;
using Prise.IntegrationTestsContract;
using Prise.Plugin;

namespace PluginE.Bootstrappers
{
    [PluginBootstrapper(PluginType = (typeof(AuthenticatedDataService)))]
    public class AuthenticationServiceBootstrapper : IPluginBootstrapper
    {
        public IServiceCollection Bootstrap(IServiceCollection services)
        {
            return services.AddTransient<ITokenService, TokenService>();
        }
    }
}
