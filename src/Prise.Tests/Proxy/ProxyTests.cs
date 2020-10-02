using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Prise.Tests
{
    [TestClass]
    public class ProxyTests
    {
        [TestMethod]
        public async Task InvokingProxyWorks()
        {
            var originalObject = new MyService();

            var proxy = Prise.Proxy.ProxyCreator.CreateProxy<IMyService>(originalObject);
            var result = await proxy.GetString();

            Assert.AreEqual("Test", result);
        }
    }
}
