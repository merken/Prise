namespace Prise.Infrastructure
{
    public interface ILocalAssemblyLoaderOptions : IAssemblyLoadOptions
    {
        string PluginPath { get; }
    }

    public class LocalAssemblyLoaderOptions : AssemblyLoadOptions, ILocalAssemblyLoaderOptions
    {
        private readonly string pluginPath;
        public LocalAssemblyLoaderOptions(string pluginPath,
            PluginPlatformVersion pluginPlatformVersion = null,
            bool ignorePlatformInconsistencies = false,
            DependencyLoadPreference dependencyLoadPreference = DependencyLoadPreference.PreferDependencyContext,
            NativeDependencyLoadPreference nativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferInstalledRuntime
        )
         : base(pluginPlatformVersion, ignorePlatformInconsistencies, dependencyLoadPreference, nativeDependencyLoadPreference)
        {
            this.pluginPath = pluginPath;
        }

        public virtual string PluginPath => pluginPath;
    }
}