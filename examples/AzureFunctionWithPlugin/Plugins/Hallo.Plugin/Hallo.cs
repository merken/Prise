using System;
using System.Threading.Tasks;
using Contract;
using Prise.Plugin;

namespace Hallo.Plugin
{
    [Plugin(PluginType = typeof(IHelloPlugin))]
    public class Hallo : IHelloPlugin
    {
        public Task<string> SayHello(string input)
        {
            return Task.FromResult($"Hallo {input}");
        }
    }
}
