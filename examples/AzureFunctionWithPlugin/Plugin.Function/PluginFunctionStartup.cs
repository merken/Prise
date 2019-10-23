using System;
using Contract;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Plugin.Function.Infrastructure;
using Prise.Infrastructure.NetCore;

[assembly: FunctionsStartup(typeof(Plugin.Function.PluginFunctionStartup))]
namespace Plugin.Function
{
    public class PluginFunctionStartup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();

            builder.Services.AddPriseWithPluginLoader<IHelloPlugin, FunctionPluginLoader>(options =>
            {
                var serverUrl = GetEnvironmentVariable("PluginServerUrl");
                options
                    .WithNetworkAssemblyLoader(serverUrl)
                    .ConfigureServices(services =>
                        services
                        .AddScoped<IPluginServerOptions>(s => new PluginServerOptions(serverUrl))
                    );
            });
        }

        public static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}