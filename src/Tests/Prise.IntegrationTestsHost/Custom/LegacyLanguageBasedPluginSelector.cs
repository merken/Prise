using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Prise.Infrastructure;

namespace Prise.IntegrationTestsHost.Custom
{
    public class LegacyLanguageBasedPluginSelector : IPluginSelector<Legacy.Domain.ITranslationPlugin>
    {
        private readonly IHttpContextAccessor contextAccessor;

        public LegacyLanguageBasedPluginSelector(IHttpContextAccessor contextAccessor)
        {
            if (contextAccessor == null)
                throw new ArgumentNullException(nameof(contextAccessor));

            this.contextAccessor = contextAccessor;
        }

        public IEnumerable<Type> SelectPlugins(IEnumerable<Type> pluginTypes)
        {
            var currentLanguage = this.contextAccessor.HttpContext.Request.Headers["Accept-Language"][0];

            switch (currentLanguage.ToLower())
            {
                case "en-gb": return pluginTypes.Where(t => t.Name == "FromEnglishTranslationPlugin");
                case "fr-fr": return pluginTypes.Where(t => t.Name == "FromFrenchTranslationPlugin");
                case "nl-be": return pluginTypes.Where(t => t.Name == "FromDutchTranslationPlugin");
            }

            throw new NotSupportedException($"Language {currentLanguage} is not supported for {typeof(Legacy.Domain.ITranslationPlugin).Name}");
        }
    }
}
