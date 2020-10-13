using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Legacy.Domain;
using Microsoft.Extensions.Configuration;

namespace LegacyPlugin1_5
{
    public abstract class TranslationPluginBase : ITranslationPlugin
    {
        protected readonly IConfiguration configuration;
        protected readonly IDictionaryService dictionaryService;

        protected TranslationPluginBase(IConfiguration configuration, IDictionaryService dictionaryService)
        {
            // This will guard us from possible nullpointers
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (dictionaryService == null)
                throw new ArgumentNullException(nameof(dictionaryService));

            this.configuration = configuration;
            this.dictionaryService = dictionaryService;
        }

        protected abstract string GetLanguageCode();

        public async Task<IEnumerable<TranslationOutput>> Translate(TranslationInput input)
        {
            if (input == null || String.IsNullOrWhiteSpace(input.ContentToTranslate))
                throw new ArgumentNullException(nameof(input));

            var languageCultureCode = this.GetLanguageCode();

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

        public Task<IEnumerable<TranslationOutput>> TranslateV2(TranslationInput input) => Translate(input);
    }
}
