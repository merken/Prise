using Example.Contract;
using Microsoft.Extensions.Configuration;

namespace Example.Akka
{
    public class AppSettingsConfigurationService: IConfigurationService
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