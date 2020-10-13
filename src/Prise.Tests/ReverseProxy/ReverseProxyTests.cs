using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Prise.Tests
{
    [TestClass]
    public class ReverseProxyTests : TestBase
    {
        [TestMethod]
        public async Task InvokingProxyWorks()
        {
            var originalObject = new MyService();

            var proxy = new MyServiceProxy(originalObject);
            var result = await proxy.GetString();

            Assert.AreEqual("Test", result);
        }
    }
}
