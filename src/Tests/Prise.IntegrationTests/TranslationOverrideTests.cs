using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Prise.IntegrationTests
{
 // These tests do not succeed (System.PlatformNotSupportedException: Named maps are not supported)
#if NETCORE3_0
    public class TranslationOverrideTests : TranslationTestsBase,
         IClassFixture<AppHostWebApplicationFactory>
    {
        public TranslationOverrideTests(
                 AppHostWebApplicationFactory factory) : base(factory.AddInMemoryConfig(new Dictionary<string, string>()
            { { "LanguageOverride", "de-DE" } })) { }

        [Theory]
        [InlineData("fr-FR", "cat", "Katze")]
        [InlineData("fr-FR", "hello", "Guten Tag")]
        [InlineData("fr-FR", "goodbye", "Auf Wiedersehen")]
        [InlineData("de-DE", "cat", "Katze")]
        [InlineData("de-DE", "hello", "Guten Tag")]
        [InlineData("de-DE", "goodbye", "Auf Wiedersehen")]
        [InlineData("nl-BE", "cat", "Katze")]
        [InlineData("nl-BE", "hello", "Guten Tag")]
        [InlineData("nl-BE", "goodbye", "Auf Wiedersehen")]
        public async Task PluginD_Works_Always_DE(string culture, string input, string result)
        {
            // Arrange, Act
            var results = await GetTranslations(_client, culture, $"/translation?&input={input}");

            Assert.Equal(1, results.Count());
            Assert.Equal(result, results.First().Translation);
        }
    }
#endif
}
