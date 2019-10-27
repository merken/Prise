using System;
using System.Collections.Generic;
using System.Linq;

namespace Contract
{
    public class LanguageInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public LanguageInfo Dialects { get; set; }
    }

    /// <summary>
    /// Using JSONConvert as IResultConverter, the return types must be POCO objects
    /// </summary>
    public class HelloDictionary
    {
        public HelloDictionary(Dictionary<string, string> dictionary, LanguageInfo[] languages)
        {
            this.Dictionary = dictionary;
            this.LanuguageInfo = languages;
        }

        public LanguageInfo[] LanuguageInfo { get; set; }
        public Dictionary<string, string> Dictionary { get; set; }
        public string[] SupportedLanguages => Dictionary.Keys.Select(k => k).ToArray();
    }
}