using Dictionary.Domain;
using Microsoft.Extensions.DependencyInjection;
using Prise.Plugin;

namespace LanguageBased.Plugin
{
    [PluginBootstrapper(PluginType = typeof(TranslationPluginWithFactory))]
    public class TranslationPluginBootstrapper : IPluginBootstrapper
    {
        public IServiceCollection Bootstrap(IServiceCollection services)
        {
            services.AddScoped<IDictionaryService, DictionaryService>();
            return services;
        }
    }
}
