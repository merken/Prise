using System;
using Contract;
using Prise.Infrastructure;

namespace Hello.Plugin
{
    [Plugin(PluginType = typeof(IHelloPlugin))]
    public class Hello : IHelloPlugin
    {
        public string SayHello(string input)
        {
            return $"Hello {input}";
        }
    }
}
