namespace Prise.Infrastructure
{
    /// <summary>
    /// Sets the preferred Native Library loading strategy
    /// </summary>
    public enum NativeDependencyLoadPreference
    {
        // Native libraries will be loaded from the runtime folder on the host
        // Windows: C:\Program Files\dotnet\shared\..
        // Linux: /usr/share/dotnet/shared/..
        // OSX: TODO
        PreferInstalledRuntime = 0,
        // Native libraries will be loaded from the remote context 
        PreferDependencyContext
    }
}
