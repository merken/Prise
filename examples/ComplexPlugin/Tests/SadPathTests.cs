using System;
using System.Net.Http;
using System.Threading.Tasks;
using AppHost.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Tests
{
    public class SadPathTests : PluginTestBase,
         IClassFixture<AppHostWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly AppHostWebApplicationFactory _factory;

        public SadPathTests(
                 AppHostWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task PluginD_DoesNotExists()
        {
            // Arrange
            var payload = new CalculationRequestModel
            {
                A = 100,
                B = 150
            };

            //Act
#if NETCORE3_0
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await Post<CalculationResponseModel>(_client, "PluginD", "/eager", payload));
#endif
#if NETCORE2_1
            await Assert.ThrowsAsync<System.Exception>(async () => await Post<CalculationResponseModel>(_client, "PluginD", "/eager", payload));
#endif
        }

        [Fact]
        public async Task PluginA_Description_Does_Not_Work()
        {
            // Arrange, Act
#if NETCORE3_0
            await Assert.ThrowsAsync<Prise.PrisePluginException>(async () => await GetRaw(_client, "PluginB", "/disco/description"));
#endif
#if NETCORE2_1
            await Assert.ThrowsAsync<System.Exception>(async () => await GetRaw(_client, "PluginB", "/disco/description"));
#endif
        }
    }
}
