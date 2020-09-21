using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Prise.Activation;
using Prise.AssemblyLoading;
using Prise.AssemblyScanning;
using Prise.Console.Contract;

[assembly: FunctionsStartup(typeof(Prise.Functions.Example.PluginFunctionsStartup))]
namespace Prise.Functions.Example
{
    public class PluginFunctionsStartup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IConfigurationService, EnvironmentConfigurationService>();
            builder.Services.AddTransient<IAssemblyScanner, DefaultAssemblyScanner>();
            builder.Services.AddTransient<IAssemblyLoader, DefaultAssemblyLoader>();
            builder.Services.AddTransient<IPluginActivator, DefaultPluginActivator>();
            builder.Services.AddTransient<IPluginLoader, FunctionPluginLoader>();
        }
    }
}