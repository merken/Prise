using System.Threading.Tasks;

namespace ExternalServices
{
    public class CurrentLanguage
    {
        public string LanguageCultureCode { get; set; }
    }

    public interface ICurrentLanguageProvider
    {
        Task<CurrentLanguage> GetCurrentLanguage();
    }
}
