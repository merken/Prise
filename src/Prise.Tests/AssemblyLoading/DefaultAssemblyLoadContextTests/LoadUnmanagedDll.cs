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
    public class LoadUnmanagedDll : Base
    {
        [TestMethod]
        public async Task Disposing_Prevents_LoadUnmanagedDll()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();

            var assemblyName = this.GetType().Assembly.GetName().Name;
            loadContext.Dispose();

            var result = (IntPtr)loadContext.GetType().GetMethod("LoadUnmanagedDll", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new[] { assemblyName });
            Assert.AreEqual(IntPtr.Zero, result);
        }

        [TestMethod]
        public async Task LoadUnmanagedDll_Works()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var assemblyLoadStrategy = testContext.GetMock<IAssemblyLoadStrategy>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            var nativeDependency = "Nativelib.dll";
            var nativePtr = new IntPtr(1024 * 10000);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            assemblyLoadStrategy.Setup(a => a.LoadUnmanagedDll(initialPluginLoadDirectory, nativeDependency, pluginDependencyContext.Object,
                It.IsAny<Func<string, string, ValueOrProceed<string>>>(),
                It.IsAny<Func<string, string, ValueOrProceed<string>>>(),
                It.IsAny<Func<string, string, ValueOrProceed<IntPtr>>>())).Returns(NativeAssembly.Create(null, nativePtr));

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(GetPluginLoadContext(pluginAssemblyPath));
            var result = (IntPtr)loadContext.GetType().GetMethod("LoadUnmanagedDll", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new[] { nativeDependency });

            Assert.IsNotNull(result);
            Assert.AreEqual(nativePtr, result);
        }
    }
}