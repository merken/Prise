using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Prise.Tests
{
    public class MyServiceProxy : Prise.Proxy.ReverseProxy, IMyService
    {
        public MyServiceProxy(object originalObject) : base(originalObject) { }

        public Task<string> GetString()
        {
            return this.InvokeThisMethodOnHostService<Task<string>>();
        }
    }

    [TestClass]
    public class ReverseProxyTests
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
