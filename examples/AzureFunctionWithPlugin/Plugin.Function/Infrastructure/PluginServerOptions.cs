namespace Plugin.Function.Infrastructure
{
    public interface IPluginServerOptions
    {
        string PluginServerUrl { get; }
    }

    public class PluginServerOptions : IPluginServerOptions
    {
        private readonly string pluginServerUrl;

        public PluginServerOptions(string pluginServerUrl)
        {
            this.pluginServerUrl = pluginServerUrl;
        }
        
        public string PluginServerUrl => this.pluginServerUrl;
    }
}