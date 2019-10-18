using System;
using Contract;
using Prise.Infrastructure;

namespace HelloWorldPlugin
{
    [Plugin(PluginType = typeof(IHelloPlugin))]
    public class HelloWorldPlugin : IHelloPlugin
    {
        public string SayHello(string input)
        {
            return $"Hallo {input}";
        }
    }
}
