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
    public class DefaultAssemblyLoadContextTests_LoadUnmanagedFromRemote : TestWithLoadedPluginBase
    {
        [TestMethod]
        public async Task LoadUnmanagedFromRemote_Returns_PathToDependency()
        {
            var testContext = await SetupLoadedPluginTextContext((plc) => plc.NativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferDependencyContext);
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var initialPluginLoadDirectory = testContext.InitialPluginLoadDirectory;

            var nativeDependency = "Nativelib";
            var pathToDependency = $"{initialPluginLoadDirectory}/Nativelib.dll";
            fileSystemUtility.Setup(f => f.DoesFileExist(pathToDependency)).Returns(true);

            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<string>>(
                loadContext,
                "LoadUnmanagedFromRemote",
                new object[] { initialPluginLoadDirectory, nativeDependency });

            Assert.IsNotNull(result.Value);
            Assert.AreEqual(pathToDependency, result.Value);
            Assert.IsFalse(result.CanProceed);
        }

        [TestMethod]
        public async Task LoadUnmanagedFromRemote_NothingFound_Returns_EmtpyAndProceed()
        {
            var testContext = await SetupLoadedPluginTextContext((plc) => plc.NativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferDependencyContext);
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var initialPluginLoadDirectory = testContext.InitialPluginLoadDirectory;

            var nativeDependency = "Nativelib";
            var pathToDependency = $"{initialPluginLoadDirectory}/Nativelib.dll";
            fileSystemUtility.Setup(f => f.DoesFileExist(pathToDependency)).Returns(false);

            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<string>>(
                loadContext,
                "LoadUnmanagedFromRemote",
                new object[] { initialPluginLoadDirectory, nativeDependency });

            Assert.AreEqual(String.Empty, result.Value);
            Assert.IsTrue(result.CanProceed);
        }
    }
}