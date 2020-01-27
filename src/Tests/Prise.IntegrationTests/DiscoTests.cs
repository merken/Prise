using System.Threading.Tasks;
using Xunit;

namespace Prise.IntegrationTests
{
    public class DiscoTests : CalculationPluginTestsBase
    {
        public DiscoTests() : base(AppHostWebApplicationFactory.Default()) { }

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
