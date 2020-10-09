using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prise.AssemblyLoading;
using System;

namespace Prise.Tests.AssemblyLoading.DefaultAssemblyLoadContextTests
{
    [TestClass]
    public class Ctor : Base
    {
        [TestMethod]
        public void Ctor_No_NativeAssemblyUnloaderFactory_Throws_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(null, null, null, null, null, null, null));
        }

        [TestMethod]
        public void Ctor_No_DependencyResolverFactory_Throws_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(() => null, null, null, null, null, null, null));
        }

        [TestMethod]
        public void Ctor_No_AssemblyLoadStrategyFactory_Throws_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(() => null, () => null, null, null, null, null, null));
        }

        [TestMethod]
        public void Ctor_No_PluginDependencyContextFactory_Throws_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(() => null, () => null, () => null, null, null, null, null));
        }


        [TestMethod]
        public void Ctor_No_FileSystemUtilitiesFactory_Throws_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(() => null, () => null, () => null, (c) => null, (s) => null, () => null, null));
        }


        [TestMethod]
        public void Ctor_Works()
        {
            Assert.IsNotNull(new DefaultAssemblyLoadContext(() => null, () => null, () => null, (c) => null, (s) => null, () => null, () => null));
        }
    }
}