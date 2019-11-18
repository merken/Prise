using System;
using Contract;
using Language.Domain;
using Prise.Plugin;

namespace Random.Plugin
{
    [Plugin(PluginType = typeof(IHelloPlugin))]
    public class LanguageBasedPlugin : IHelloPlugin
    {
        private readonly ILanguageService languageService;
        private readonly ISharedLanguageService sharedLanguageService;
        protected LanguageBasedPlugin(ILanguageService languageService, ISharedLanguageService sharedLanguageService)
        {
            this.languageService = languageService;
            this.sharedLanguageService = sharedLanguageService;
        }

        [PluginFactory]
        public static LanguageBasedPlugin ThisIsTheFactoryMethod(IServiceProvider serviceProvider) =>
            new LanguageBasedPlugin(
                (ILanguageService)serviceProvider.GetService(typeof(ILanguageService)),
                (ISharedLanguageService)serviceProvider.GetService(typeof(ISharedLanguageService))
            );

        public string SayHello(string input)
        {
            if (this.sharedLanguageService == null)
                throw new Exception("sharedLanguageService is null");
            if (this.languageService == null)
                throw new Exception("languageService is null");
            var language = this.sharedLanguageService.GetLanguage();
            var dictionary = this.languageService.GetLanguageDictionary();

            if (dictionary.ContainsKey(language))
                return $"{dictionary[language]} {input}";

            return $"We could not find a suitable word for language {language}";
        }
    }
}
