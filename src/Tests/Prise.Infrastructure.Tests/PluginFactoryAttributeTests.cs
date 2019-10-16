using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Prise.Infrastructure.Tests
{
    [TestClass]
    public class PluginFactoryAttributeTests
    {
        [TestMethod]
        public void PluginTypeMustBeCorrect()
        {
            // Arrange
            var pluginFactoryAttribute = new PluginFactoryAttribute();

            // Act, Assert
            Assert.IsNotNull(pluginFactoryAttribute);
        }
    }
}
