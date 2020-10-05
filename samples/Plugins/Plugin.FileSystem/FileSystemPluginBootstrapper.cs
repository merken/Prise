using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Example.Contract;
using Prise.Plugin;

namespace Plugin.FileSystem
{
    [Prise.Plugin.PluginBootstrapper(PluginType = typeof(FileSystemPlugin))]
    public class FileSystemPluginBootstrapper : Prise.Plugin.IPluginBootstrapper
    {
        [BootstrapperService(ServiceType = typeof(IConfigurationService), ProxyType =  typeof(ConfigurationService))]
        private readonly IConfigurationService configurationService;

        public IServiceCollection Bootstrap(IServiceCollection services)
        {
            return services
                .AddSingleton<IConfigurationService>(this.configurationService) // Add the injected service as singleton
                .AddTransient<IFileSystemService, FileSystemService>(); // The FileSystemService will be injected with the IConfigurationService
        }
    }
}