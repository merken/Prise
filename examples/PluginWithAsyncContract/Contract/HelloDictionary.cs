using System;
using System.Collections.Generic;
using System.Linq;

namespace Contract
{
    [Serializable]
    public class HelloDictionary
    {
        Dictionary<string, string> dictionary;

        public HelloDictionary(Dictionary<string, string> dictionary)
        {
            this.dictionary = dictionary;
        }

        public string[] SupportedLanugages => this.dictionary.Keys.Select(k => k).ToArray();
    }
}