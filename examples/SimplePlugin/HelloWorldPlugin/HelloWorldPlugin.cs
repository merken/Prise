using System;
using Contract;
using Prise.Plugin;

namespace HelloWorldPlugin
{
    [Plugin(PluginType = typeof(IHelloWorldPlugin))]
    public class HelloWorldPlugin : IHelloWorldPlugin
    {
        public string SayHello(string input)
        {
            return $"Hello {input}";
        }
    }
}
