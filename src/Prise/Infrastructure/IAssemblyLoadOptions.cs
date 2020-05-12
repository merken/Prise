namespace Prise.Infrastructure
{
    public interface IAssemblyLoadOptions<T>
    {
        PluginPlatformVersion PluginPlatformVersion { get; }
        bool IgnorePlatformInconsistencies { get; }
        bool UseCollectibleAssemblies { get; }
        NativeDependencyLoadPreference NativeDependencyLoadPreference { get; }
    }
}