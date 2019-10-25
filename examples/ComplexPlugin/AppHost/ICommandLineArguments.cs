using Microsoft.Extensions.Configuration;

namespace AppHost
{
    public interface ICommandLineArguments
    {
        bool UseLazyService { get; }
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

    }
}