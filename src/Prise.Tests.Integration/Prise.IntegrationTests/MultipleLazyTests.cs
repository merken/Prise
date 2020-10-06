using System.Net.Http;
using System.Threading.Tasks;
using Prise.IntegrationTestsHost.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Prise.IntegrationTests
{
    public class MultipleLazyTests : CalculationPluginTestsBase
    {
        public MultipleLazyTests() : base(AppHostWebApplicationFactory.Default()) { }

        [Fact]
        public async Task PluginA_Works()
        {
            // Arrange
            var payload = new CalculationRequestModel
            {
                A = 100,
                B = 150
            };

            //Act
            var result = await Post<CalculationResponseModel>(_client, "PluginA", "/multiple-lazy", payload);

            // Assert (100 + 150) + (100 + 150 + 1)
            Assert.Equal(501, result.Result);
        }

        [Fact]
        public async Task PluginB_Works()
        {
            // Arrange
            var payload = new CalculationRequestModel
            {
                A = 150,
                B = 50
            };

            //Act
            var result = await Post<CalculationResponseModel>(_client, "PluginB", "/multiple-lazy", payload);

            // Assert 150 - 50
            Assert.Equal(100, result.Result);
        }

        [Fact]
        public async Task PluginC_Works()
        {
            // Arrange
            var payload = new CalculationRequestModel
            {
                A = 50,
                B = 2
            };

            //Act
            var result = await Post<CalculationResponseModel>(_client, "PluginC", "/multiple-lazy", payload);

            // Assert 50 * 2 + 10% discount
            Assert.Equal(110, result.Result);
        }
    }
}
