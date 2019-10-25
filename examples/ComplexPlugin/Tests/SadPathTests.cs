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
            await Assert.ThrowsAsync<System.IO.FileNotFoundException>(async () => await Post<CalculationResponseModel>(_client, "PluginD", "/eager", payload));
        }

        [Fact]
        public async Task Disco_Works()
        {
            // Arrange
            var payload = new CalculationRequestModel
            {
                A = 100,
                B = 150
            };

            //Act
            await Assert.ThrowsAsync<System.IO.FileNotFoundException>(async () => await Post<CalculationResponseModel>(_client, "PluginD", "/eager", payload));
        }
    }
}
