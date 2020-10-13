using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Prise.IntegrationTestsContract;

namespace Prise.IntegrationTests
{
    public abstract class TranslationTestsBase : PluginTestBase
    {
        protected TranslationTestsBase(AppHostWebApplicationFactory factory) : base(factory) { }
        protected async Task<IEnumerable<TranslationOutput>> GetTranslations(HttpClient client, string languageCode, string endpoint)
        {
            client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
                throw new Exception("Result was not success!");

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<TranslationOutput>>(responseContent);
        }
    }
}
