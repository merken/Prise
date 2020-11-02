using Akka.Actor;
using Prise;
using System;
using Example.Contract;
using Example.Akka.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Example.Akka
{
    // An actor is singleton
    public class PluginsActor : ReceiveActor
    {
        public PluginsActor(IAkkaHostConfiguration hostConfiguration, IServiceProvider serviceProvider)
        {
            ReceiveAsync<GetAllCommand>(async command =>
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var pluginLoader = scope.ServiceProvider.GetService<IPluginLoader>();
                    var sharedConfigurationService = scope.ServiceProvider.GetService<IConfigurationService>();

                    var pathToDist = hostConfiguration.GetPathToDist();
                    var scanResult = await pluginLoader.FindPlugin<IPlugin>(pathToDist, command.Plugin);
                    if (scanResult == null)
                        throw new NotSupportedException($"This actor can not load the plugin {command.Plugin}");
                        
                    var plugin = await pluginLoader.LoadPlugin<IPlugin>(scanResult,configure: (context) =>
                    {
                        context.AddHostService<IConfigurationService>(sharedConfigurationService);
                    });

                    var result = await plugin.GetAll();
                    Sender.Tell(result);
                }
            });
        }
    }
}