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
    public class DefaultAssemblyLoadContextTests_LoadUnmanagedDll : TestWithLoadedPluginBase
    {
        [TestMethod]
        public async Task Disposing_Prevents_LoadUnmanagedDll()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();

            var assemblyName = this.GetType().Assembly.GetName().Name;
            loadContext.Dispose();

            var result = InvokeProtectedMethodOnLoadContextAndGetResult<IntPtr>(
                            loadContext,
                            "LoadUnmanagedDll",
                            new object[] { assemblyName });

            Assert.AreEqual(IntPtr.Zero, result);
        }

        [TestMethod]
        public async Task LoadUnmanagedDll_Works()
        {
            var testContext = await SetupLoadedPluginTextContext();
            var loadContext = testContext.Sut();
            var initialPluginLoadDirectory = testContext.InitialPluginLoadDirectory;
            var assemblyLoadStrategy = testContext.GetMock<IAssemblyLoadStrategy>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();

            var nativeDependency = "Nativelib.dll";
            var nativePtr = new IntPtr(1024 * 10000);

            assemblyLoadStrategy.Setup(a => a.LoadUnmanagedDll(initialPluginLoadDirectory, nativeDependency, pluginDependencyContext.Object,
                It.IsAny<Func<string, string, ValueOrProceed<string>>>(),
                It.IsAny<Func<string, string, ValueOrProceed<string>>>(),
                It.IsAny<Func<string, string, ValueOrProceed<IntPtr>>>())).Returns(NativeAssembly.Create(null, nativePtr));

            var result = InvokeProtectedMethodOnLoadContextAndGetResult<IntPtr>(
                            loadContext,
                            "LoadUnmanagedDll",
                            new object[] { nativeDependency });

            Assert.IsNotNull(result);
            Assert.AreEqual(nativePtr, result);
        }
    }
}