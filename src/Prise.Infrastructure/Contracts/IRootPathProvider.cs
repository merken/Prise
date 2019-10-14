namespace Prise.Infrastructure
{
    public interface IRootPathProvider
    {
        string GetRootPath();
    }

    public class RootPathProvider : IRootPathProvider
    {
        private readonly string rootPath;

        public RootPathProvider(string rootPath)
        {
            this.rootPath = rootPath;
        }

        public string GetRootPath() => this.rootPath;
    }
}