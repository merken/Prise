using Xunit;

namespace Prise.IntegrationTests
{
    [CollectionDefinition("AppHost collection")]
    public class AppHostCollection : ICollectionFixture<AppHostWebApplicationFactory>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
