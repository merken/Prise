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
    public class LoadUnmanagedFromRemote : Base
    {
        [TestMethod]
        public async Task LoadUnmanagedFromRemote_Returns_PathToDependency()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var runtimeDefaultAssemblyLoadContext = testContext.GetMock<IRuntimeDefaultAssemblyContext>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();
            var dependencyResolver = testContext.GetMock<IAssemblyDependencyResolver>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            var nativeDependency = "Nativelib";
            var pathToDependency = $"{initialPluginLoadDirectory}/Nativelib.dll";
            fileSystemUtility.Setup(f => f.DoesFileExist(pathToDependency)).Returns(true);

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(
                GetPluginLoadContext(pluginAssemblyPath,
                (plc) => plc.NativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferDependencyContext));
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
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var runtimeDefaultAssemblyLoadContext = testContext.GetMock<IRuntimeDefaultAssemblyContext>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();
            var dependencyResolver = testContext.GetMock<IAssemblyDependencyResolver>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            var nativeDependency = "Nativelib";
            var pathToDependency = $"{initialPluginLoadDirectory}/Nativelib.dll";
            fileSystemUtility.Setup(f => f.DoesFileExist(pathToDependency)).Returns(false);

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(
                GetPluginLoadContext(pluginAssemblyPath,
                (plc) => plc.NativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferDependencyContext));
            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<string>>(
                loadContext,
                "LoadUnmanagedFromRemote",
                new object[] { initialPluginLoadDirectory, nativeDependency });

            Assert.AreEqual(String.Empty, result.Value);
            Assert.IsTrue(result.CanProceed);
        }
    }
}