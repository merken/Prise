using System.Collections.Generic;

namespace Language.Domain
{
    public interface ILanguageService
    {
        Dictionary<string, string> GetLanguageDictionary();
    }

    public class LanguageService : ILanguageService
    {
        public Dictionary<string, string> GetLanguageDictionary()
        {
            return new Dictionary<string, string>(){
                {"en-US", "Hello"},
                {"en-GB", "Hello"},
                {"nl-BE", "Hallo"},
                {"nl-NL", "Hallo"},
                {"fr-BE", "Bonjour"},
                {"fr-FR", "Bonjour"}
            };
        }
    }
}
