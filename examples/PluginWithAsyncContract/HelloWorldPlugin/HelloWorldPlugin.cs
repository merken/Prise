using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Contract;
using Newtonsoft.Json;
using Prise.Infrastructure;

namespace HelloWorldPlugin
{
    [Plugin(PluginType = typeof(IHelloWorldPlugin))]
    public class HelloWorldPlugin : IHelloWorldPlugin
    {
        public async Task<string> SayHelloAsync(string language, string input)
        {
            var dictionary = await GetDictionary();
            if (dictionary.ContainsKey(language.ToLower()))
                return $"{dictionary[language.ToLower()]} {input}";
            throw new NotSupportedException($"Language {language} not found in dictionary");
        }

        public async Task<HelloDictionary> GetHelloDictionaryAsync()
        {
            var dictionary = await GetDictionary();
            return new HelloDictionary(dictionary);
        }

        private async Task<Dictionary<string, string>> GetDictionary()
        {
            using (var stream = new StreamReader("Plugins\\Dictionary.json"))
            {
                var json = await stream.ReadToEndAsync();
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
        }
    }
}
