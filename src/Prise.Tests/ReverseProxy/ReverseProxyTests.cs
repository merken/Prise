using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prise.Proxy;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Prise.Tests
{
    [TestClass]
    public class ReverseProxyTests : TestBase
    {
        [TestMethod]
        public async Task InvokingGetStringWorks()
        {
            var result = await GetSut().GetString();

            Assert.AreEqual("Test", result);
        }

        [TestMethod]
        public async Task InvokingSetStringWorks()
        {
            var sut = GetSut();
            sut.SetString("A string");
            var result = await sut.GetString();

            Assert.AreEqual("A string", result);
        }

        [TestMethod]
        public async Task InvokingAddWorks()
        {
            var result = GetSut().Add(33.33m, 100);

            Assert.AreEqual(133.33m, result);
        }

        [TestMethod]
        public async Task InvokingAddOverloadingWorks()
        {
            var result = GetSut().Add(33.33m, 100, 33.33m);

            Assert.AreEqual(166.66m, result);
        }

        [TestMethod]
        public async Task InvokingSubtractWorks()
        {
            var result = GetSut().Subtract(33.33m, 100m);

            Assert.AreEqual(-66.67m, result);
        }

        [TestMethod]
        public async Task InvokingMultiplyWorks()
        {
            var result = GetSut().Multiply(20, 50);

            Assert.AreEqual(1000, result);
        }

        [TestMethod]
        public async Task InvokingDivideWorks()
        {
            var result = GetSut().Divide(40.40m, 2);

            Assert.AreEqual(20.20m, result);
        }

        [TestMethod]
        public async Task InvokingDivideAsyncWorks()
        {
            var result = await GetSut().DivideAsync(40.40m, 2);

            Assert.AreEqual(20.20m, result);
        }

        [TestMethod]
        public async Task InvokingDivideThrowsException()
        {
            Assert.ThrowsException<DivideByZeroException>(() => GetSut().Divide(1000m, 0));
        }

        [TestMethod]
        public async Task InvokingDivideAsyncThrowsException()
        {
            var exception = await Assert.ThrowsExceptionAsync<AggregateException>(() => GetSut().DivideAsync(1000m, 0));
            Assert.IsInstanceOfType(exception.InnerException, typeof(DivideByZeroException));
        }

        [TestMethod]
        public async Task InvokingThrowsMyServiceExceptionWorks()
        {
            Assert.ThrowsException<MyServiceException>(() => GetSut().ThrowsMyServiceException());
        }

        [TestMethod]
        public async Task InvokingThrowsMyServiceExceptionAsyncWorks()
        {
            var exception = await Assert.ThrowsExceptionAsync<AggregateException>(async () => await GetSut().ThrowsMyServiceExceptionAsync());
            Assert.IsInstanceOfType(exception.InnerException, typeof(MyServiceException));
        }

        [TestMethod]
        public async Task ReadFromDiskWorks()
        {
            Assert.AreEqual("This is content from a test file", await GetSut().ReadFromDisk("TestFile.txt"));
        }

        [TestMethod]
        public async Task ReadFromDiskThrowsFileNotFoundException()
        {
            var exception = await Assert.ThrowsExceptionAsync<AggregateException>(() => GetSut().ReadFromDisk("NOTFOUND.txt"));
            Assert.IsInstanceOfType(exception.InnerException, typeof(FileNotFoundException));
        }

        [TestMethod]
        public async Task InvokingSetStringOverloadWorks()
        {
            var sut = GetSut();
            sut.SetString("A string");
            await sut.SetStringOverload();
            var result = await sut.GetString();

            Assert.AreEqual("A string A string", result);
        }

        [TestMethod]
        public async Task InvokingSetStringOverloadWithValueWorks()
        {
            var value = "MyValue";
            var result = await GetSut().SetStringOverload(value);

            Assert.AreEqual("MyValue", value);
        }

        [TestMethod]
        public async Task InvokingSetStringOverloadWithWrongParametersThrowsReverseProxyException()
        {
            var sut = new MyServiceProxyParameterMismatch(new MyService());
            await Assert.ThrowsExceptionAsync<ReverseProxyException>(async () => await sut.SetStringOverload("value"));
        }

        [TestMethod]
        public async Task InvokingSetStringOverloadWithValueAndInheritanceWorks()
        {
            var sut = new MyServiceProxyParameterMismatch(new MyService());
            sut.SetString("A string");
            await sut.SetStringOverload();
            var result = await sut.GetString();

            Assert.AreEqual("A string A string", result);
        }

        private IMyService GetSut() => new MyServiceProxy(new MyService());
    }
}
