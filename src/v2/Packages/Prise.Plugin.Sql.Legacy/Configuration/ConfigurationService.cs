using Prise.Console.Contract;
using Prise.PluginBridge;

namespace Prise.Plugin.Sql.Legacy.Configuration
{
    public class ConfigurationService : PluginBridgeBase, IConfigurationService
    {
        public ConfigurationService(object hostService) : base(hostService) { }

        public string GetConfigurationValueForKey(string key)
        {
            return this.InvokeThisMethodOnHostService<string>(new[] { key });
        }
    }
}