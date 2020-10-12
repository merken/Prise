using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prise.AssemblyLoading;
using System;

namespace Prise.Tests.AssemblyLoading.DefaultAssemblyLoadContextTests
{
    [TestClass]
    public class DefaultAssemblyLoadContextTests_Ctor : Base
    {
        [TestMethod]
        public void Ctor_Throws_ArgumentNullException_nativeAssemblyUnloaderFactory()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(null, null, null, null, null, null, null));
            exception.Message.Contains("nativeAssemblyUnloaderFactory");
        }

        [TestMethod]
        public void Ctor_Throws_ArgumentNullException_pluginDependencyResolverFactory()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(() => null, null, null, null, null, null, null));
            exception.Message.Contains("pluginDependencyResolverFactory");
        }

        [TestMethod]
        public void Ctor_Throws_ArgumentNullException_assemblyLoadStrategyFactory()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(() => null, () => null, null, null, null, null, null));
            exception.Message.Contains("assemblyLoadStrategyFactory");
        }

        [TestMethod]
        public void Ctor_Throws_ArgumentNullException_assemblyDependencyResolverFactory()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(() => null, () => null, () => null, null, null, null, null));
            exception.Message.Contains("assemblyDependencyResolverFactory");
        }


        [TestMethod]
        public void Ctor_Throws_ArgumentNullException_fileSystemUtilitiesFactory()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(() => null, () => null, () => null, (c) => null, null, null, null));
            exception.Message.Contains("fileSystemUtilitiesFactory");
        }

        [TestMethod]
        public void Ctor_Throws_ArgumentNullException_runtimeDefaultAssemblyLoadContextFactory()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(() => null, () => null, () => null, (c) => null, () => null, null, null));
            exception.Message.Contains("runtimeDefaultAssemblyLoadContextFactory");
        }


        [TestMethod]
        public void Ctor_Throws_ArgumentNullException_pluginDependencyContextProviderFactory()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(() => null, () => null, () => null, (c) => null, () => null, () => null, null));
            exception.Message.Contains("pluginDependencyContextProviderFactory");
        }

        [TestMethod]
        public void Ctor_Works()
        {
            Assert.IsNotNull(new DefaultAssemblyLoadContext(() => null, () => null, () => null, (c) => null, () => null, () => null, () => null));
        }
    }
}