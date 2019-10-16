using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Prise.Infrastructure.NetCore.Tests.SinglePluginLoaderTests
{
    [TestClass]
    public class LoadAll
    {
        [TestMethod]
        public void LoadAllShouldThrowException()
        {
            // Arrange
            var sut = new SinglePluginLoader<LoadAll>(new Mock<IPluginLoadOptions<LoadAll>>().Object);

            // Act, Assert
            Assert.ThrowsExceptionAsync<NotImplementedException>(() => sut.LoadAll());
        }
    }
}