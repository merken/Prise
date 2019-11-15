namespace Prise.Infrastructure.NetCore
{
    public interface ILocalAssemblyLoaderOptions : IAssemblyLoadOptions
    {
        string PluginPath { get; }
    }

    public class LocalAssemblyLoaderOptions : AssemblyLoadOptions, ILocalAssemblyLoaderOptions
    {
        private readonly string pluginPath;
        public LocalAssemblyLoaderOptions(string pluginPath,
            PluginPlatformVersion pluginPlatformVersion,
            DependencyLoadPreference dependencyLoadPreference,
            NativeDependencyLoadPreference nativeDependencyLoadPreference
        )
         : base(pluginPlatformVersion, dependencyLoadPreference, nativeDependencyLoadPreference)
        {
            this.pluginPath = pluginPath;
        }

        public string PluginPath => pluginPath;
    }
}