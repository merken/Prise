namespace Prise.Infrastructure
{
    public interface IPluginAssemblyNameProvider
    {
        string GetAssemblyName();
    }

    public class PluginAssemblyNameProvider : IPluginAssemblyNameProvider
    {
        private readonly string assemblyName;

        public PluginAssemblyNameProvider(string assemblyName)
        {
            this.assemblyName = assemblyName;
        }

        public string GetAssemblyName() => this.assemblyName;
    }
}