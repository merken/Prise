using Microsoft.Extensions.Configuration;

namespace Prise.IntegrationTestsHost
{
    public interface ICommandLineArguments
    {
        bool UseLazyService { get; }
        bool UseCollectibleAssemblies { get; }
    }

    public class CommandLineArguments : ICommandLineArguments
    {
        private readonly IConfiguration config;
        public CommandLineArguments(IConfiguration config)
        {
            this.config = config;
        }

        public bool UseLazyService
        {
            get
            {
                if (bool.TryParse(this.config["uselazyservice"], out var uselazyservice))
                    return uselazyservice;
                return false;
            }
        }

        public bool UseCollectibleAssemblies
        {
            get
            {
                if (bool.TryParse(this.config["usecollectibleassemblies"], out var usecollectibleassemblies))
                    return usecollectibleassemblies;
                return false;
            }
        }
    }
}