using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Prise.Tests
{
    public class MyServiceBridge : PluginBridge.PluginBridge, IMyService
    {
        public MyServiceBridge(object originalObject) : base(originalObject) { }

        public Task<string> GetString()
        {
            return this.InvokeThisMethodOnHostService<Task<string>>();
        }
    }

    [TestClass]
    public class PluginBridgeTests
    {
        [TestMethod]
        public async Task InvokingProxyWorks()
        {
            var originalObject = new MyService();

            var bridge = new MyServiceBridge(originalObject);
            var result = await bridge.GetString();

            Assert.AreEqual("Test", result);
        }
    }
}
