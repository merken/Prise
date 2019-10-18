using System;
using Contract;
using Prise.Infrastructure;

namespace Bonjour.Plugin
{
    [Plugin(PluginType = typeof(IHelloPlugin))]
    public class Bonjour : IHelloPlugin
    {
        public string SayHello(string input)
        {
            return $"Bonjour {input}";
        }
    }
}
