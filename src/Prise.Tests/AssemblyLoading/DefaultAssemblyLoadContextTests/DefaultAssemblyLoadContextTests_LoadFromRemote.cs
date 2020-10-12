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
    public class DefaultAssemblyLoadContextTests_LoadFromRemote : TestWithLoadedPluginBase
    {
        [TestMethod]
        public async Task LoadFromRemote_Found_Returns_Assembly()
        {
            var testContext = await SetupLoadedPluginTextContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var runtimeDefaultAssemblyLoadContext = testContext.GetMock<IRuntimeDefaultAssemblyContext>();
            var initialPluginLoadDirectory = testContext.InitialPluginLoadDirectory;
            var pluginLoadContext = testContext.PluginLoadContext;
            var newtonsoftAssemblyName = testContext.NewtonsoftAssemblyName;
            var newtonsoftAssembly = testContext.NewtonsoftAssembly;
            var newtonsoftAssemblyPath = testContext.NewtonsoftAssemblyPath;
            var resolver = testContext.GetMock<IAssemblyDependencyResolver>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();

            var newtonsoftAssemblyFileName = Path.GetFileName(newtonsoftAssemblyPath);
            var newtonsoftAssemblyStream = File.OpenRead(newtonsoftAssemblyPath);
            var newtonsoftAssemblyBytes = File.ReadAllBytes(newtonsoftAssemblyPath);

            fileSystemUtility.Setup(f => f.DoesFileExist($"{GetPathToAssemblies()}/{newtonsoftAssemblyFileName}")).Returns(true);
            fileSystemUtility.Setup(f => f.ReadDependencyFileFromDisk(GetPathToAssemblies(), newtonsoftAssemblyFileName)).Returns(newtonsoftAssemblyStream);
            fileSystemUtility.Setup(f => f.ToByteArray(newtonsoftAssemblyStream)).Returns(newtonsoftAssemblyBytes);

            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<AssemblyFromStrategy>>(
                            loadContext,
                            "LoadFromRemote",
                            new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName });
                            
            Assert.IsNotNull(result);
            Assert.AreEqual(newtonsoftAssemblyName.Name, result.Value.Assembly.GetName().Name);
        }

        [TestMethod]
        public async Task LoadFromRemote_NothingFound_Returns_Proceed()
        {
            var testContext = await SetupLoadedPluginTextContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var runtimeDefaultAssemblyLoadContext = testContext.GetMock<IRuntimeDefaultAssemblyContext>();
            var initialPluginLoadDirectory = testContext.InitialPluginLoadDirectory;
            var pluginLoadContext = testContext.PluginLoadContext;
            var newtonsoftAssemblyName = testContext.NewtonsoftAssemblyName;
            var newtonsoftAssembly = testContext.NewtonsoftAssembly;
            var newtonsoftAssemblyPath = testContext.NewtonsoftAssemblyPath;
            var resolver = testContext.GetMock<IAssemblyDependencyResolver>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();

            fileSystemUtility.Setup(f => f.DoesFileExist($"{GetPathToAssemblies()}/Newtonsoft.Json.dll")).Returns(false);

            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<AssemblyFromStrategy>>(
                            loadContext,
                            "LoadFromRemote",
                            new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.CanProceed);
        }
    }
}