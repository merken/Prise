using Prise.Infrastructure;

namespace Prise.Infrastructure.NetCore.Contracts
{
    public interface ILocalAssemblyLoaderOptions : IAssemblyLoadOptions
    {
        string PluginPath { get; }
    }

    public class LocalAssemblyLoaderOptions : AssemblyLoadOptions, ILocalAssemblyLoaderOptions
    {
        private readonly string pluginPath;
        public LocalAssemblyLoaderOptions(string pluginPath, DependencyLoadPreference dependencyLoadPreference = DependencyLoadPreference.PreferRemote)
         : base(dependencyLoadPreference)
        {
            this.pluginPath = pluginPath;
        }
        
        public string PluginPath => pluginPath;
    }
}