using System;
using System.Threading.Tasks;
using Contract;
using Prise.Plugin;

namespace Hello.Plugin
{
    [Plugin(PluginType = typeof(IHelloPlugin))]
    public class Hello : IHelloPlugin
    {
        public Task<string> SayHello(string input)
        {
            return Task.FromResult($"Hello {input}");
        }
    }
}
