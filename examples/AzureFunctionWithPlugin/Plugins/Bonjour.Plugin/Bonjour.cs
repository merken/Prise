using System;
using System.Threading.Tasks;
using Contract;
using Prise.Plugin;

namespace Bonjour.Plugin
{
    [Plugin(PluginType = typeof(IHelloPlugin))]
    public class Bonjour : IHelloPlugin
    {
        public Task<string> SayHello(string input)
        {
            return Task.FromResult($"Bonjour {input}");
        }
    }
}
