
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prise.AssemblyLoading;
using Prise.AssemblyScanning;
using Prise.Core;
using Prise.Tests.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise.Tests.AssemblyLoading
{
    [TestClass]
    public class DefaultAssemblyLoaderTests : TestBase
    {
        [TestMethod]
        public void Ctor_No_AssemblyLoadContextFactory_Throws_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoader(null));
        }

        [TestMethod]
        public void Ctor_Works()
        {
            Assert.IsNotNull(new DefaultAssemblyLoader(() => null));
        }

        [TestMethod]
        public async Task Load_No_Context_Throws_ArgumentNullException()
        {
            var loader = new DefaultAssemblyLoader(() => null);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => loader.Load(null));
        }

        [TestMethod]
        public async Task Load_No_PathToAssembly_Throws_ArgumentNullException()
        {
            var loader = new DefaultAssemblyLoader(() => null);
            var loadContext = new PluginLoadContext("Path To Plugin", this.GetType(), "netcoreapp3.1");
            loadContext.FullPathToPluginAssembly = null;
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => loader.Load(loadContext));
        }

        [TestMethod]
        public async Task Load_UnRooted_PathToAssembly_Throws_ArgumentNullException()
        {
            var loader = new DefaultAssemblyLoader(() => null);
            var loadContext = new PluginLoadContext("../testpath", this.GetType(), "netcoreapp3.1");
            await Assert.ThrowsExceptionAsync<AssemblyLoadingException>(() => loader.Load(loadContext));
        }

        [TestMethod]
        public async Task Load_Works()
        {
            var mockLoadContext = this.mockRepository.Create<IAssemblyLoadContext>();
            var assemblyShim = this.mockRepository.Create<IAssemblyShim>();
            var loader = new DefaultAssemblyLoader(() => mockLoadContext.Object);
            var loadContext = new PluginLoadContext("/home/maarten/assembly.dll", this.GetType(), "netcoreapp3.1");
            mockLoadContext.Setup(c => c.LoadPluginAssembly(loadContext)).ReturnsAsync(assemblyShim.Object);

            var assembly = await loader.Load(loadContext);

            Assert.AreEqual(assemblyShim.Object, assembly);
        }

        [TestMethod]
        public async Task Unload_No_Context_Throws_ArgumentNullException()
        {
            var loader = new DefaultAssemblyLoader(() => null);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => loader.Unload(null));
        }

        [TestMethod]
        public async Task Unload_No_PathToAssembly_Throws_ArgumentNullException()
        {
            var loader = new DefaultAssemblyLoader(() => null);
            var loadContext = new PluginLoadContext("Path To Plugin", this.GetType(), "netcoreapp3.1");
            loadContext.FullPathToPluginAssembly = null;
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => loader.Unload(loadContext));
        }

        [TestMethod]
        public async Task Unload_UnRooted_PathToAssembly_Throws_ArgumentNullException()
        {
            var loader = new DefaultAssemblyLoader(() => null);
            var loadContext = new PluginLoadContext("../testpath", this.GetType(), "netcoreapp3.1");
            await Assert.ThrowsExceptionAsync<AssemblyLoadingException>(() => loader.Unload(loadContext));
        }

        [TestMethod]
        public async Task Unload_Works()
        {
            var mockLoadContext = this.mockRepository.Create<IAssemblyLoadContext>();
            var assemblyShim = this.mockRepository.Create<IAssemblyShim>();
            var loader = new DefaultAssemblyLoader(() => mockLoadContext.Object);
            var loadContext = new PluginLoadContext("/home/maarten/assembly.dll", this.GetType(), "netcoreapp3.1");
            mockLoadContext.Setup(c => c.LoadPluginAssembly(loadContext)).ReturnsAsync(assemblyShim.Object);

            var assembly = await loader.Load(loadContext);
            await loader.Unload(loadContext);

            Assert.AreEqual(assemblyShim.Object, assembly);
        }
    }
}