using System;
using System.Threading.Tasks;

namespace Prise.Console.Contract
{
    public class PluginObject
    {
        public int Number { get; set; }
        public string Text { get; set; }
    }

    public interface IPlugin
    {
        Task<PluginObject> GetData(PluginObject input);
    }
}
