using Microsoft.Extensions.Configuration;

namespace MyHost.Infrastructure
{
    public interface ICommandLineArguments
    {
        bool UseNetwork { get; }
    }

    public class CommandLineArguments : ICommandLineArguments
    {
        private readonly IConfiguration config;
        public CommandLineArguments(IConfiguration config)
        {
            this.config = config;
        }

        public bool UseNetwork
        {
            get
            {
                if (bool.TryParse(this.config["usenetwork"], out var usenetwork))
                    return usenetwork;
                return false;
            }
        }
    }
}