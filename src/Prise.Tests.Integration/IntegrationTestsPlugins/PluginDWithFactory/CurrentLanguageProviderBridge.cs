using System.Threading.Tasks;
using ExternalServices;
using Prise.PluginBridge;

namespace LanguageBased.Plugin
{
    public class CurrentLanguageProviderBridge : ICurrentLanguageProvider
    {
        private readonly object hostService;
        public CurrentLanguageProviderBridge(object hostService)
        {
            this.hostService = hostService;
        }

        public Task<CurrentLanguage> GetCurrentLanguage()
        {
            var methodInfo = typeof(ICurrentLanguageProvider).GetMethod(nameof(GetCurrentLanguage));
            return PrisePluginBridge.Invoke(this.hostService, methodInfo) as Task<CurrentLanguage>;
        }
    }
}
