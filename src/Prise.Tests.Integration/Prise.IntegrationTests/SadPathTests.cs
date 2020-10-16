using System;
using System.Net.Http;
using System.Threading.Tasks;
using Prise.IntegrationTestsHost.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Prise.IntegrationTests
{
    public class SadPathTests : CalculationPluginTestsBase
    {
        public SadPathTests() : base(AppHostWebApplicationFactory.Default()) { }

        [Fact]
        public async Task PluginZ_DoesNotExists()
        {
            // Arrange
            var payload = new CalculationRequestModel
            {
                A = 100,
                B = 150
            };

            //Act
#if NETCORE3_0 || NETCORE3_1
            await Assert.ThrowsAsync<System.IO.DirectoryNotFoundException>(async () => await Post<CalculationResponseModel>(_client, "PluginZ", "/calculation", payload));
#endif
#if NETCORE2_1
            await Assert.ThrowsAsync<System.Exception>(async () => await Post<CalculationResponseModel>(_client, "PluginZ", "/calculation", payload));
#endif
        }

        [Fact]
        public async Task PluginB_Description_Does_Not_Work()
        {
            // Arrange, Act
#if NETCORE3_0 || NETCORE3_1
            await Assert.ThrowsAsync<Prise.Proxy.PriseProxyException>(async () => await GetRaw(_client, "PluginB", "/disco/description"));
#endif
#if NETCORE2_1
            await Assert.ThrowsAsync<System.Exception>(async () => await GetRaw(_client, "PluginB", "/disco/description"));
#endif
        }
    }
}
