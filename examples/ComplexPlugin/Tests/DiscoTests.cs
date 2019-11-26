using System;
using System.Net.Http;
using System.Threading.Tasks;
using AppHost.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Tests
{
    public class DiscoTests : PluginTestBase,
         IClassFixture<AppHostWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly AppHostWebApplicationFactory _factory;

        public DiscoTests(
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
            // Arrange, Act
            var result = await GetRaw(_client, "PluginA", "/disco");

            // Assert
            Assert.Equal("AdditionCalculationPlugin,ZAdditionPlusOneCalculationPlugin", result);
        }

        [Fact]
        public async Task PluginA_Description_Works()
        {
            // Arrange, Act
            
            var result = await GetRaw(_client, "PluginA", "/disco/description");

            // Assert
            Assert.Equal("This plugin performs addition,This plugin performs addition +1", result);
        }

        [Fact]
        public async Task PluginB_Works()
        {
            // Arrange, Act
            var result = await GetRaw(_client, "PluginB", "/disco");

            // Assert
            Assert.Equal("SubtractionCalculationPlugin", result);
        }

        [Fact]
        public async Task PluginC_Works()
        {
            // Arrange, Act
            var result = await GetRaw(_client, "PluginC", "/disco");

            // Assert
            Assert.Equal("DivideOrMultiplyCalculationPlugin", result);
        }

        [Fact]
        public async Task PluginC_Description_Works()
        {
            // Arrange, Act
            var result = await GetRaw(_client, "PluginC", "/disco/description");

            // Assert
            Assert.Equal("This plugin performs division OR multiplication, check out DivideOrMultiplyCalculationBootstrapper for more details", result);
        }
    }
}
