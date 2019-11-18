using System;
using Contract;
using Prise.Plugin;
using Random.Domain;

namespace Random.Plugin
{
    [Plugin(PluginType = typeof(IHelloPlugin))]
    public class RandomPlugin : IHelloPlugin
    {
        private readonly IRandomService service;
        protected RandomPlugin(IRandomService service)
        {
            this.service = service;
        }

        [PluginFactory]
        public static RandomPlugin ThisIsTheFactoryMethod(IServiceProvider serviceProvider) =>
            new RandomPlugin((IRandomService)serviceProvider.GetService(typeof(IRandomService)));

        public string SayHello(string input)
        {
            var number = this.service.ProvideRandomNumber();

            if (number <= 25)
                return $"Ola {input}";

            if (number <= 50)
                return $"Gutentäg {input}";

            if (number <= 75)
                return $"Oi {input}";

            if (number <= 100)
                return $"Aloha {input}";

            return "Random service did not provide a number between 0 and 100";
        }
    }
}
