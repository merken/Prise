using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Prise.IntegrationTestsContract;
using Xunit;

namespace Prise.IntegrationTests
{
    public class DataControllerTests : PluginTestBase,
     IClassFixture<AppHostWebApplicationFactory>
    {
        public DataControllerTests(
                 AppHostWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task Get_Token_Works()
        {
            // Arrange, Act
            // PluginC is the Calc plugin type and is required for the controller to be instantiated
            var result = await GetToken(_client, "PluginC", "/data");

            // Assert
            Assert.NotEqual(Guid.Empty, new Guid(result));
        }

        [Fact]
        public async Task Get_DataV1_Works()
        {
            // Arrange, Act
            var token = await GetToken(_client, "PluginC", "/data");
            var result = await GetData(_client, "nl-BE", "PluginC", $"/data/{token}/v1");

            // Assert
            Assert.Equal(2, result.Count());
        }

#if NETCORE3_0
        [Fact]
        public async Task Get_Data_Works()
        {
            // Arrange, Act
            var token = await GetToken(_client, "PluginC", "/data");
            var result = await GetData(_client, "nl-BE", "PluginC", $"/data/{token}");

            // Assert
            Assert.Equal(2, result.Count());
        }
#endif

        protected async Task<string> GetToken(HttpClient client, string pluginType, string endpoint)
        {
            client.DefaultRequestHeaders.Add("PluginType", pluginType);
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
                throw new Exception("Result was not success!");

            return await response.Content.ReadAsStringAsync();
        }

        protected async Task<IEnumerable<Data>> GetData(
            HttpClient client,
            string languageCode,
            string pluginType,
            string endpoint)
        {
            client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
            client.DefaultRequestHeaders.Add("PluginType", pluginType);
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
                throw new Exception("Result was not success!");

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<Data>>(responseContent);
        }
    }
}
