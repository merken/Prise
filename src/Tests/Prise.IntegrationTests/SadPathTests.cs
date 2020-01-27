using System;
using System.Net.Http;
using System.Threading.Tasks;
using Prise.IntegrationTestsHost.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Prise.IntegrationTests
{
    public class SadPathTests : CalculationPluginTestsBase,
         IClassFixture<AppHostWebApplicationFactory>
    {
        public SadPathTests(
                 AppHostWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task PluginG_DoesNotExists()
        {
            // Arrange
            var payload = new CalculationRequestModel
            {
                A = 100,
                B = 150
            };

            //Act
#if NETCORE3_0
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await Post<CalculationResponseModel>(_client, "PluginG", "/eager", payload));
#endif
#if NETCORE2_1
            await Assert.ThrowsAsync<System.Exception>(async () => await Post<CalculationResponseModel>(_client, "PluginG", "/eager", payload));
#endif
        }

        [Fact]
        public async Task PluginA_Description_Does_Not_Work()
        {
            // Arrange, Act
#if NETCORE3_0
            await Assert.ThrowsAsync<Prise.Proxy.Exceptions.PriseProxyException>(async () => await GetRaw(_client, "PluginB", "/disco/description"));
#endif
#if NETCORE2_1
            await Assert.ThrowsAsync<System.Exception>(async () => await GetRaw(_client, "PluginB", "/disco/description"));
#endif
        }
    }
}
