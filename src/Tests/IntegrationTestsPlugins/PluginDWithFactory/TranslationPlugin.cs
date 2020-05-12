using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dictionary.Domain;
using ExternalServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prise.IntegrationTestsContract;
using Prise.Plugin;

namespace LanguageBased.Plugin
{
    [Plugin(PluginType = typeof(ITranslationPlugin))]
    public class TranslationPluginWithFactory : ITranslationPlugin
    {
        private readonly IConfiguration configuration;
        private readonly IDictionaryService dictionaryService;

        internal TranslationPluginWithFactory(IConfiguration configuration, IDictionaryService dictionaryService)
        {
            // This will guard us from possible nullpointers
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (dictionaryService == null)
                throw new ArgumentNullException(nameof(dictionaryService));

            this.configuration = configuration;
            this.dictionaryService = dictionaryService;
        }

        [PluginFactory]
        public static TranslationPluginWithFactory ThisIsTheFactoryMethod(IServiceProvider serviceProvider)
        {
            var configFromHost = serviceProvider.GetService<IConfiguration>();
            var dictionaryService = serviceProvider.GetService<IDictionaryService>();

            return new TranslationPluginWithFactory(
                configFromHost, // This service is provided by the Prise.IntegrationTestsHost application and is registered as a Host Type
                dictionaryService // This service is provided by the plugin using the PluginBootstrapper
            );
        }

        public async Task<IEnumerable<TranslationOutput>> Translate(TranslationInput input)
        {
            var languageCultureCode = "de-DE";

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
