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
    public class LoadUnmanagedFromDefault : Base
    {
        [TestMethod]
        public async Task LoadUnmanagedFromDefault_NotFound_Returns_ZeroPtrAndProceed()
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

            var nativeDependency = "not-found";
            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(
                GetPluginLoadContext(pluginAssemblyPath,
                (plc) => plc.NativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferDependencyContext));
            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<IntPtr>>(
                loadContext,
                "LoadUnmanagedFromDefault",
                new object[] { initialPluginLoadDirectory, nativeDependency });

            Assert.AreEqual(IntPtr.Zero, result.Value);
            Assert.IsTrue(result.CanProceed);
        }
    }
}