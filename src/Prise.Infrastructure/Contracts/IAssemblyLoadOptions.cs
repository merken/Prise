namespace Prise.Infrastructure
{
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