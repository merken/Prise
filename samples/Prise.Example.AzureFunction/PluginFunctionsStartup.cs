using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Prise.Activation;
using Prise.AssemblyLoading;
using Prise.AssemblyScanning;
using Prise.Example.Contract;
using Prise.DependencyInjection;

[assembly: FunctionsStartup(typeof(Prise.Example.AzureFunction.PluginFunctionsStartup))]
namespace Prise.Example.AzureFunction
{
    public class PluginFunctionsStartup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IConfigurationService, EnvironmentConfigurationService>();
            builder.Services.AddPrise();
            builder.Services.AddTransient<IPluginLoader, FunctionPluginLoader>();
        }
    }
}