using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prise.AssemblyLoading;
using Prise.Core;
using Prise.Tests.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace Prise.Tests.AssemblyLoading.DefaultAssemblyLoadContextTests
{
    [TestClass]
    public class DefaultAssemblyLoadContextTests_LoadUnmanagedFromDefault : TestWithLoadedPluginBase
    {
        [TestMethod]
        public async Task LoadUnmanagedFromDefault_NotFound_Returns_ZeroPtrAndProceed()
        {
            var testContext = await SetupLoadedPluginTextContext((plc) => plc.NativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferDependencyContext);
            var loadContext = testContext.Sut();
            var initialPluginLoadDirectory = testContext.InitialPluginLoadDirectory;

            var nativeDependency = "not-found";
            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<IntPtr>>(
                loadContext,
                "LoadUnmanagedFromDefault",
                new object[] { initialPluginLoadDirectory, nativeDependency });

            Assert.AreEqual(IntPtr.Zero, result.Value);
            Assert.IsTrue(result.CanProceed);
        }
    }
}