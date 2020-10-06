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
        [PluginService(ServiceType = typeof(IConfiguration), ProvidedBy = ProvidedBy.Host)]
        private readonly IConfiguration configuration;
        [PluginService(
            ServiceType = typeof(ICurrentLanguageProvider),
            ProvidedBy = ProvidedBy.Host, 
            BridgeType = typeof(CurrentLanguageProviderBridge))]
        private readonly ICurrentLanguageProvider languageProvider;
        [PluginService(ServiceType = typeof(IDictionaryService))]
        private readonly IDictionaryService dictionaryService;

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
