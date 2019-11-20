using System;
using System.Collections.Generic;
using Contract;
using Prise.Plugin;

namespace Random.Plugin
{
    [Plugin(PluginType = typeof(IHelloPlugin))]
    public class LanguageBasedPlugin : IHelloPlugin
    {
        private readonly IExternalService externalService;
        protected LanguageBasedPlugin(IExternalService externalService)
        {
            this.externalService = externalService;
        }

        [PluginFactory]
        public static LanguageBasedPlugin ThisIsTheFactoryMethod(IServiceProvider serviceProvider) =>
            new LanguageBasedPlugin(
                (IExternalService)serviceProvider.GetService(typeof(IExternalService)) // This service is provided by the MyHost application
            );

        public string SayHello(string input)
        {
            if (this.externalService == null)
                throw new Exception("externalService is null");

            var language = this.externalService.GetLanguage();
            var dictionary = GetLanguageDictionary();

            if (dictionary.ContainsKey(language))
                return $"{dictionary[language]} {input}";

            return $"We could not find a suitable word for language {language}";
        }

        private Dictionary<string, string> GetLanguageDictionary()
        {
            return new Dictionary<string, string>(){
                {"en-US", "Hello"},
                {"en-GB", "Hello"},
                {"nl-BE", "Hallo"},
                {"nl-NL", "Hallo"},
                {"fr-BE", "Bonjour"},
                {"fr-FR", "Bonjour"}
            };
        }
    }
}
