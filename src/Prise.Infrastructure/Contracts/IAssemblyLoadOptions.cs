namespace Prise.Infrastructure
{
    /// <summary>
    /// Sets the preferred dependency loading strategy
    /// </summary>
    public enum DependencyLoadPreference
    {
        // Dependencies will first be loaded from default load context
        PreferDependencyContext = 0,
        // Dependencies will first be loaded from the remote location
        PreferRemote,
        // Dependencies will first be loaded from the host app domain
        PreferAppDomain
    }

    public interface IAssemblyLoadOptions
    {
        DependencyLoadPreference DependencyLoadPreference { get; }
    }

    public class AssemblyLoadOptions : IAssemblyLoadOptions
    {
        private readonly DependencyLoadPreference dependencyLoadPreference;
        public AssemblyLoadOptions(DependencyLoadPreference dependencyLoadPreference = DependencyLoadPreference.PreferRemote)
        {
            this.dependencyLoadPreference = dependencyLoadPreference;
        }

        public DependencyLoadPreference DependencyLoadPreference
        {
            get
            {
                return this.dependencyLoadPreference;
            }
        }
    }
}