using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Prise.IntegrationTests
{
    // These tests do not succeed (System.PlatformNotSupportedException: Named maps are not supported)
#if NETCORE3_0
    public class TranslationTests : TranslationTestsBase,
         IClassFixture<AppHostWebApplicationFactory>
    {
        public TranslationTests(
                 AppHostWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task PluginD_DE_Works()
        {
            // Arrange, Act
            var results = await GetTranslations(_client, "de-DE", "/translation?&input=dog");

            // Assert
            Assert.Equal(2, results.Count());
        }

        [Fact]
        public async Task PluginD_FR_Works()
        {
            // Arrange, Act
            var results = await GetTranslations(_client, "fr-FR", "/translation?&input=dog");

            // Assert
            Assert.Equal(2, results.Count());
        }

        [Fact]
        public async Task PluginD_NL_Works()
        {
            // Arrange, Act
            var results = await GetTranslations(_client, "nl-BE", "/translation?&input=dog");

            // Assert
            Assert.Equal(2, results.Count());
        }

        [Fact]
        public async Task PluginD_EN_Returns_Empty_List()
        {
            // Arrange, Act
            var results = await GetTranslations(_client, "en-GB", "/translation?&input=dog");

            // Assert
            Assert.Equal(0, results.Count());
        }

        [Theory]
        [InlineData("fr-FR", "cat", "Chat")]
        [InlineData("fr-FR", "hello", "Bonjour")]
        [InlineData("fr-FR", "goodbye", "Au revoir")]
        [InlineData("de-DE", "cat", "Katze")]
        [InlineData("de-DE", "hello", "Guten Tag")]
        [InlineData("de-DE", "goodbye", "Auf Wiedersehen")]
        [InlineData("nl-BE", "cat", "Kat")]
        [InlineData("nl-BE", "hello", "Hallo")]
        [InlineData("nl-BE", "goodbye", "Tot ziens")]
        public async Task PluginD_Works(string culture, string input, string result)
        {
            // Arrange, Act
            var results = await GetTranslations(_client, culture, $"/translation?&input={input}");

            // Assert
            Assert.Equal(1, results.Count());
            Assert.Equal(result, results.First().Translation);
        }
    }
#endif
}
