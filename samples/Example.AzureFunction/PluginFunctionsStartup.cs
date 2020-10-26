using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Example.Contract;
using Prise.DependencyInjection;

[assembly: FunctionsStartup(typeof(Example.AzureFunction.PluginFunctionsStartup))]
namespace Example.AzureFunction
{
    public class PluginFunctionsStartup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IConfigurationService, EnvironmentConfigurationService>();
            builder.Services.AddPrise();
        }
    }
}