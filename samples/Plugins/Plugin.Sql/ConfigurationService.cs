using Example.Contract;
using Prise.Proxy;

namespace Plugin.Sql
{
    public class ConfigurationService : ReverseProxy, IConfigurationService
    {
        public ConfigurationService(object hostService) : base(hostService) { }

        public string GetConfigurationValueForKey(string key)
        {
            return this.InvokeOnHostService<string>(key);
        }
    }
}