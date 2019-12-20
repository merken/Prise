namespace Prise.Infrastructure
{
    public interface IPluginLoadContext
    {
        string PluginAssemblyName { get; }
        string PluginAssemblyPath { get; }
    }
}
