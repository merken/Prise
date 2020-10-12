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
    public class DefaultAssemblyLoadContextTests_LoadPluginAssembly : Base
    {
        [TestMethod]
        public async Task Load_No_Context_Throws_ArgumentNullException()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => loadContext.LoadPluginAssembly(null));
        }

        [TestMethod]
        public async Task Load_No_PathToAssembly_Throws_ArgumentNullException()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var pluginLoadContext = new PluginLoadContext("Path To Plugin", this.GetType(), "netcoreapp3.1");
            pluginLoadContext.FullPathToPluginAssembly = null;
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => loadContext.LoadPluginAssembly(pluginLoadContext));
        }

        [TestMethod]
        public async Task Load_UnRooted_PathToAssembly_Throws_ArgumentNullException()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var pluginLoadContext = new PluginLoadContext("../testpath", this.GetType(), "netcoreapp3.1");
            await Assert.ThrowsExceptionAsync<AssemblyLoadingException>(() => loadContext.LoadPluginAssembly(pluginLoadContext));
        }

        [TestMethod]
        public async Task LoadPluginAssembly_Works()
        {
            var pluginAssemblyPath = "/var/home/MyPluginAssembly.dll";
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var pluginDependencyContextProvider = testContext.GetMock<IPluginDependencyContextProvider>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();

            var contract = TestableTypeBuilder.NewTestableType()
                .WithName("IMyTestType")
                .WithNamespace("Test.Type")
                .Build();

            var assembly = this.GetType().Assembly;
            var assemblyStream = File.OpenRead(assembly.Location);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);
            var pluginLoadContext = new PluginLoadContext(pluginAssemblyPath, contract, "netcoreapp3.1");
            pluginDependencyContextProvider.Setup(p => p.FromPluginLoadContext(pluginLoadContext)).ReturnsAsync(pluginDependencyContext.Object);

            var priseAssembly = await loadContext.LoadPluginAssembly(pluginLoadContext);

            Assert.AreEqual(assembly.FullName, priseAssembly.Assembly.FullName);
        }

        [TestMethod]
        public async Task LoadPluginAssembly_Guard_Works()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var pluginAssemblyPath = "/var/home/MyPluginAssembly.dll";
            var pluginDependencyContextProvider = testContext.GetMock<IPluginDependencyContextProvider>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();

            var contract = TestableTypeBuilder.NewTestableType()
                .WithName("IMyTestType")
                .WithNamespace("Test.Type")
                .Build();

            var assembly = this.GetType().Assembly;
            var assemblyStream = File.OpenRead(assembly.Location);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            var pluginLoadContext = new PluginLoadContext(pluginAssemblyPath, contract, "netcoreapp3.1");
            pluginDependencyContextProvider.Setup(p => p.FromPluginLoadContext(pluginLoadContext)).ReturnsAsync(pluginDependencyContext.Object);

            var priseAssembly = await loadContext.LoadPluginAssembly(pluginLoadContext);

            await Assert.ThrowsExceptionAsync<AssemblyLoadingException>(() => loadContext.LoadPluginAssembly(pluginLoadContext));
        }
    }
}