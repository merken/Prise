using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Prise.Infrastructure.Tests
{
    class MyPluginType { }

    [TestClass]
    public class PluginAttributeTests
    {
        [TestMethod]
        public void PluginTypeMustBeCorrect()
        {
            // Arrange
            var pluginAttribute = new PluginAttribute() { PluginType = typeof(MyPluginType) };

            // Act, Assert
            Assert.AreEqual(typeof(MyPluginType), pluginAttribute.PluginType);
        }
    }
}
