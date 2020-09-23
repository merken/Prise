using Prise.Example.Contract;

namespace Prise.Example.Plugin.Sql
{
    public class ConfigurationService : Prise.PluginBridge.PluginBridge, IConfigurationService
    {
        public ConfigurationService(object hostService) : base(hostService) { }

        public string GetConfigurationValueForKey(string key)
        {
            return this.InvokeThisMethodOnHostService<string>(new[] { key });
        }
    }
}