namespace Prise.Infrastructure.NetCore.Contracts
{
    public interface ILocalAssemblyLoaderOptions
    {
        string PluginPath { get; }
    }

    public class LocalAssemblyLoaderOptions : ILocalAssemblyLoaderOptions
    {
        private readonly string pluginPath;
        public LocalAssemblyLoaderOptions(string pluginPath)
        {
            this.pluginPath = pluginPath;

        }
        public string PluginPath => pluginPath;
    }
}