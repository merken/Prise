using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dictionary.Domain;
using ExternalServices;
using Microsoft.Extensions.Configuration;
using Prise.IntegrationTestsContract;
using Prise.Plugin;

namespace LanguageBased.Plugin
{
    [Plugin(PluginType = typeof(ITranslationPlugin))]
    public class TranslationPlugin : ITranslationPlugin
    {
        private readonly IConfiguration configuration;
        private readonly ICurrentLanguageProvider languageProvider;
        private readonly IDictionaryService dictionaryService;

        internal TranslationPlugin(IConfiguration configuration, ICurrentLanguageProvider languageProvider, IDictionaryService dictionaryService)
        {
            // This will guard us from possible nullpointers
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (languageProvider == null)
                throw new ArgumentNullException(nameof(languageProvider));
            if (dictionaryService == null)
                throw new ArgumentNullException(nameof(dictionaryService));

            this.configuration = configuration;
            this.languageProvider = languageProvider;
            this.dictionaryService = dictionaryService;
        }

        [PluginFactory]
        public static TranslationPlugin ThisIsTheFactoryMethod(IServiceProvider pluginServices, IServiceProvider hostServices)
        {
            var configFromHost = hostServices.GetService<IConfiguration>();

            var hostService = hostServices.GetService(typeof(ICurrentLanguageProvider));
            var hostServiceBridge = new CurrentLanguageProviderBridge(hostService);

            var dictionaryService = pluginServices.GetService<IDictionaryService>();

            return new TranslationPlugin(
                configFromHost, // This service is provided by the Prise.IntegrationTestsHost application and is registered as a Host Type
                hostServiceBridge, // This service is provided by the Prise.IntegrationTestsHost application and is registered as a Remote Type
                dictionaryService // This service is provided by the plugin using the PluginBootstrapper
            );
        }

        public async Task<IEnumerable<TranslationOutput>> Translate(TranslationInput input)
        {
            // Reaches out into the host to get the current language
            var currentLanguage = await this.languageProvider.GetCurrentLanguage();
            var languageCultureCode = currentLanguage.LanguageCultureCode;

            // Reaches out into the host to check the config for override
            var languageFromConfig = this.configuration["LanguageOverride"];
            if (!String.IsNullOrEmpty(languageFromConfig))
                languageCultureCode = languageFromConfig;

            // Uses a local third party service
            var dictionary = await dictionaryService.GetLanguageDictionary(languageCultureCode);
            if (dictionary == null)
                return Enumerable.Empty<TranslationOutput>();

            var wordToTranslate = input.ContentToTranslate.ToLower();

            if (!dictionary.ContainsKey(wordToTranslate))
                return Enumerable.Empty<TranslationOutput>();

            var results = new List<TranslationOutput>();
            foreach (var key in dictionary.Keys.Where(k => k.ToLower().StartsWith(wordToTranslate)))
            {
                var translation = dictionary[key];
                var accuracy = key.Length == wordToTranslate.Length ? 100 : 0;
                if (accuracy == 0)
                    accuracy = 100 / (wordToTranslate.Length - key.Length);

                results.Add(new TranslationOutput
                {
                    Accuracy = accuracy,
                    ToLanguage = languageCultureCode,
                    Translation = translation
                });
            }

            return results;
        }
    }
}
