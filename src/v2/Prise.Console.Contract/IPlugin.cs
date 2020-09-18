using System;
using System.Threading.Tasks;

namespace Prise.Console.Contract
{
    public class PluginObject
    {
        public int Number { get; set; }
        public string Text { get; set; }
    }

    public interface IConfigurationService
    {
        string GetConfigurationValueForKey(string key);
    }

    public interface IPlugin
    {
        Task<PluginObject> GetData(PluginObject input);
        // Task<PluginObject> SaveData(PluginObject input);
    }

    public interface IMultiplePlugin : IPlugin
    {
    }

    public interface IStoragePlugin : IPlugin
    {
    }
}
