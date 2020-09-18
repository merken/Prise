using Microsoft.Extensions.Configuration;
using Prise.Console.Contract;

namespace Prise.Web
{
    public class AppSettingsConfigurationService : IConfigurationService
    {
        private readonly IConfiguration configuration;

        public AppSettingsConfigurationService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string GetConfigurationValueForKey(string key)
        {
            return configuration[key];
        }
    }
}