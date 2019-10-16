using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Prise.Infrastructure.Tests
{
    [TestClass]
    public class RootPathProviderTests
    {
        [TestMethod]
        public void ShouldProvideRootPath()
        {
            // Arrange
            var provider = new RootPathProvider("my root path");

            // Act, Assert
            Assert.AreEqual("my root path", provider.GetRootPath());
        }
    }
}
