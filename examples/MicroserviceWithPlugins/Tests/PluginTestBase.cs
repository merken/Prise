using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;

namespace Tests
{
    public class PluginTestBase :
#if NETCORE2_1
         IClassFixture<MyHost2WebApplicationFactory>
#endif
#if NETCORE3_0
         IClassFixture<MyHostWebApplicationFactory>
#endif
    {
        protected readonly HttpClient _client;
#if NETCORE2_1
        protected readonly MyHost2WebApplicationFactory _factory;
#endif
#if NETCORE3_0
        protected readonly MyHostWebApplicationFactory _factory;
#endif

        public PluginTestBase(
#if NETCORE2_1
         MyHost2WebApplicationFactory factory
#endif
#if NETCORE3_0
         MyHostWebApplicationFactory factory
#endif
            )
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        protected async Task<T> Post<T>(HttpClient client, string tenant, string endpoint, object content)
        {
            client.DefaultRequestHeaders.Add("Tenant", tenant);
            var response = await client.PostAsync(endpoint, new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8,
                                                "application/json"));
            if (!response.IsSuccessStatusCode)
                throw new Exception("Result was not success!");

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseContent);
        }

        protected async Task<T> Get<T>(HttpClient client, string tenant, string endpoint, bool starwars = false)
        {
            client.DefaultRequestHeaders.Add("Tenant", tenant);
            if(starwars)
                client.DefaultRequestHeaders.Add("starwars", "yes");

            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
                throw new Exception("Result was not success!");
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseContent);
        }

        protected async Task<string> GetRaw(HttpClient client, string tenant, string endpoint)
        {
            client.DefaultRequestHeaders.Add("Tenant", tenant);
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
                throw new Exception("Result was not success!");

            return await response.Content.ReadAsStringAsync();
        }
    }
}