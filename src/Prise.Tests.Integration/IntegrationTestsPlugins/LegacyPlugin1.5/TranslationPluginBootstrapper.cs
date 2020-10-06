using Legacy.Domain;
using Microsoft.Extensions.DependencyInjection;
using Prise.Plugin;

namespace LegacyPlugin1_5
{
    [PluginBootstrapper(PluginType = typeof(FromEnglishTranslationPlugin))]
    [PluginBootstrapper(PluginType = typeof(FromFrenchTranslationPlugin))]
    [PluginBootstrapper(PluginType = typeof(FromDutchTranslationPlugin))]
    public class TranslationPluginBootstrapper : IPluginBootstrapper
    {
        public IServiceCollection Bootstrap(IServiceCollection services)
        {
            services.AddScoped<IDictionaryService, DictionaryService>();
            return services;
        }
    }
}
