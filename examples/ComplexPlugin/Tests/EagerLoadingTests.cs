using System.Net.Http;
using System.Threading.Tasks;
using AppHost.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Tests
{
    public class EagerLoadingTests : PluginTestBase,
         IClassFixture<AppHostWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly AppHostWebApplicationFactory _factory;

        public EagerLoadingTests(
                 AppHostWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

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
            var result = await Post<CalculationResponseModel>(_client, "PluginA", "/eager", payload);

            // Assert 100 + 150
            Assert.Equal(250, result.Result);
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
            var result = await Post<CalculationResponseModel>(_client, "PluginB", "/eager", payload);

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
            var result = await Post<CalculationResponseModel>(_client, "PluginC", "/eager", payload);

            // Assert 50 * 2 + 10% discount
            Assert.Equal(110, result.Result);
        }

        [Fact]
        public async Task PluginA_int_Works()
        {
            // Arrange
            var payload = new CalculationRequestModel
            {
                A = 100,
                B = 150
            };

            //Act
            var result = await Post<CalculationResponseModel>(_client, "PluginA", "/eager/int", payload);

            // Assert 100 + 150
            Assert.Equal(250, result.Result);
        }

        [Fact]
        public async Task PluginB_int_Works()
        {
            // Arrange
            var payload = new CalculationRequestModel
            {
                A = 150,
                B = 50
            };

            //Act
            var result = await Post<CalculationResponseModel>(_client, "PluginB", "/eager/int", payload);

            // Assert 150 - 50
            Assert.Equal(100, result.Result);
        }

        [Fact]
        public async Task PluginC_int_Works()
        {
            // Arrange
            var payload = new CalculationRequestModel
            {
                A = 50,
                B = 2
            };

            //Act
            var result = await Post<CalculationResponseModel>(_client, "PluginC", "/eager/int", payload);

            // Assert 50 * 2 + 10% discount
            Assert.Equal(110, result.Result);
        }

        [Fact]
        public async Task PluginA_complex_input_Works()
        {
            // Arrange
            var payload = new CalculationRequestModel
            {
                A = 100,
                B = 150
            };

            //Act
            var result = await Post<CalculationResponseModel>(_client, "PluginA", "/eager/complex-input", payload);

            // Assert 100 + 150
            Assert.Equal(250, result.Result);
        }

        [Fact]
        public async Task PluginB_complex_input_Works()
        {
            // Arrange
            var payload = new CalculationRequestModel
            {
                A = 150,
                B = 50
            };

            //Act
            var result = await Post<CalculationResponseModel>(_client, "PluginB", "/eager/complex-input", payload);

            // Assert 150 - 50
            Assert.Equal(100, result.Result);
        }

        [Fact]
        public async Task PluginC_complex_input_Works()
        {
            // Arrange
            var payload = new CalculationRequestModel
            {
                A = 50,
                B = 2
            };

            //Act
            var result = await Post<CalculationResponseModel>(_client, "PluginC", "/eager/complex-input", payload);

            // Assert 50 * 2 + 10% discount
            Assert.Equal(110, result.Result);
        }

        [Fact]
        public async Task PluginA_complex_output_Works()
        {
            // Arrange
            var payload = new CalculationRequestModel
            {
                A = 100,
                B = 150
            };

            //Act
            var result = await Post<CalculationResponseModel>(_client, "PluginA", "/eager/complex-output", payload);

            // Assert 100 + 150
            Assert.Equal(250, result.Result);
        }

        [Fact]
        public async Task PluginB_complex_output_Works()
        {
            // Arrange
            var payload = new CalculationRequestModel
            {
                A = 150,
                B = 50
            };

            //Act
            var result = await Post<CalculationResponseModel>(_client, "PluginB", "/eager/complex-output", payload);

            // Assert 150 - 50
            Assert.Equal(100, result.Result);
        }

        [Fact]
        public async Task PluginC_complex_output_Works()
        {
            // Arrange
            var payload = new CalculationRequestModel
            {
                A = 50,
                B = 2
            };

            //Act
            var result = await Post<CalculationResponseModel>(_client, "PluginC", "/eager/complex-output", payload);

            // Assert 50 * 2 + 10% discount
            Assert.Equal(110, result.Result);
        }

        [Fact]
        public async Task PluginA_multi_Works()
        {
            // Arrange
            var payload = new CalculationRequestMultiModel
            {
                Calculations = new[]
                {
                    new CalculationRequestModel
                    {
                        A = 100,
                        B = 150
                    },
                    new CalculationRequestModel
                    {
                        A = 100,
                        B = 150
                    }
                }
            };

            //Act
            var result = await Post<CalculationResponseModel>(_client, "PluginA", "/eager/multi", payload);

            // Assert 100 + 150 + 100 + 150
            Assert.Equal(500, result.Result);
        }

        [Fact]
        public async Task PluginB_multi_Works()
        {
            // Arrange
            var payload = new CalculationRequestMultiModel
            {
                Calculations = new[]
                {
                    new CalculationRequestModel
                    {
                        A = 50,
                        B = 5
                    },
                    new CalculationRequestModel
                    {
                        A = 40,
                        B = 5
                    }
                }
            };

            //Act
            var result = await Post<CalculationResponseModel>(_client, "PluginB", "/eager/multi", payload);

            // Assert (50 - 5) + (40 - 5)
            Assert.Equal(80, result.Result);
        }

        [Fact]
        public async Task PluginC_multi_Works()
        {
            // Arrange
            var payload = new CalculationRequestMultiModel
            {
                Calculations = new[]
                {
                    new CalculationRequestModel
                    {
                        A = 50,
                        B = 2
                    },
                    new CalculationRequestModel
                    {
                        A = 40,
                        B = 2
                    }
                }
            };

            //Act
            var result = await Post<CalculationResponseModel>(_client, "PluginC", "/eager/multi", payload);

            // Assert (50 * 2 + 10% discount) + (40 * 2 + 10% discount)
            Assert.Equal(198, result.Result);
        }
    }
}
