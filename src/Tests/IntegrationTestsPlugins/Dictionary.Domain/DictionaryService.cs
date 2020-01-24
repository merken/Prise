using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Dictionary.Domain
{
    public interface IDictionaryService
    {
        Task<Dictionary<string, string>> GetLanguageDictionary(string culture);
    }

    public class DictionaryService : IDictionaryService
    {
        public async Task<Dictionary<string, string>> GetLanguageDictionary(string culture)
        {
            var filePath = Path.Combine(GetLocalExecutionPath(), $"dictionary-{culture}.json");
            if (!File.Exists(filePath))
                return null;

            // Reads the dictionary.json file from wherever host is running
            using (var stream = new StreamReader(filePath))
            {
                var json = await stream.ReadToEndAsync();
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
        }

        private string GetLocalExecutionPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
