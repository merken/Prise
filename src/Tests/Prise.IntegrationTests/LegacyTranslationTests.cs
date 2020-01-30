using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Prise.IntegrationTests
{
    public class LegacyTranslationTests : TranslationTestsBase
    {
        public LegacyTranslationTests() : base(AppHostWebApplicationFactory.Default()) { }

        [Theory]
        [InlineData("nl-BE", 2)]
        [InlineData("fr-FR", 2)]
        [InlineData("en-GB", 0)]
        public async Task Plugin1_4_Works(string language, int resultCount)
        {
            // Arrange, Act
            var results = await GetTranslations(_client, language, "/translation/legacy?&version=1.4&input=dog");

            // Assert
            Assert.Equal(resultCount, results.Count());
        }

        [Theory]
        [InlineData("nl-BE", 2)]
        [InlineData("fr-FR", 2)]
        [InlineData("en-GB", 0)]
        public async Task Plugin1_5_Works(string language, int resultCount)
        {
            // Arrange, Act
            var results = await GetTranslations(_client, language, "/translation/legacy?&version=1.5&input=dog");

            // Assert
            Assert.Equal(resultCount, results.Count());
        }
    }
}
