namespace Prise.Infrastructure
{
    public interface IAssemblyLoadOptions<T>
    {
        PluginPlatformVersion PluginPlatformVersion { get; }
        bool IgnorePlatformInconsistencies { get; }
        NativeDependencyLoadPreference NativeDependencyLoadPreference { get; }
    }
}