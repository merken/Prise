using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Prise.Infrastructure.Tests
{
    [TestClass]
    public class PluginBootstrapperAttributeTests
    {
        [TestMethod]
        public void PluginTypeMustBeCorrect()
        {
            // Arrange
            var pluginBootstrapperAttribute = new PluginBootstrapperAttribute() { PluginType = typeof(MyPluginType) };

            // Act, Assert
            Assert.AreEqual(typeof(MyPluginType), pluginBootstrapperAttribute.PluginType);
        }
    }
}
