using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tests
{
    public class PluginTestBase
    {
        protected async Task<T> Post<T>(HttpClient client, string pluginType, string endpoint, object content)
        {
            client.DefaultRequestHeaders.Add("PluginType", pluginType);
            var response = await client.PostAsync(endpoint, new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8,
                                                "application/json"));
            if (!response.IsSuccessStatusCode)
                throw new Exception("Result was not success!");

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseContent);
        }

        protected async Task<string> GetRaw(HttpClient client, string pluginType, string endpoint)
        {
            client.DefaultRequestHeaders.Add("PluginType", pluginType);
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
                throw new Exception("Result was not success!");

            return await response.Content.ReadAsStringAsync();
        }
    }
}