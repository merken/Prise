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
    public class DefaultAssemblyLoadContextTests : TestBase
    {
        [TestMethod]
        public void Ctor_No_NativeAssemblyUnloaderFactory_Throws_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(null, null, null, null, null, null));
        }

        [TestMethod]
        public void Ctor_No_DependencyResolverFactory_Throws_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(() => null, null, null, null, null, null));
        }

        [TestMethod]
        public void Ctor_No_AssemblyLoadStrategyFactory_Throws_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(() => null, () => null, null, null, null, null));
        }

        [TestMethod]
        public void Ctor_No_PluginDependencyContextFactory_Throws_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(() => null, () => null, () => null, null, null, null));
        }

        [TestMethod]
        public void Ctor_Works()
        {
            Assert.IsNotNull(new DefaultAssemblyLoadContext(() => null, () => null, () => null, (c) => null, (s) => null, () => null));
        }

        [TestMethod]
        public async Task Load_No_Context_Throws_ArgumentNullException()
        {
            var loadContext = new DefaultAssemblyLoadContext(() => null, () => null, () => null, (c) => null, (s) => null, () => null);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => loadContext.LoadPluginAssembly(null));
        }

        [TestMethod]
        public async Task Load_No_PathToAssembly_Throws_ArgumentNullException()
        {
            var loadContext = new DefaultAssemblyLoadContext(() => null, () => null, () => null, (c) => null, (s) => null, () => null);
            var pluginLoadContext = new PluginLoadContext("Path To Plugin", this.GetType(), "netcoreapp3.1");
            pluginLoadContext.FullPathToPluginAssembly = null;
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => loadContext.LoadPluginAssembly(pluginLoadContext));
        }

        [TestMethod]
        public async Task Load_UnRooted_PathToAssembly_Throws_ArgumentNullException()
        {
            var loadContext = new DefaultAssemblyLoadContext(() => null, () => null, () => null, (c) => null, (s) => null, () => null);
            var pluginLoadContext = new PluginLoadContext("../testpath", this.GetType(), "netcoreapp3.1");
            await Assert.ThrowsExceptionAsync<AssemblyLoadingException>(() => loadContext.LoadPluginAssembly(pluginLoadContext));
        }
    }
}