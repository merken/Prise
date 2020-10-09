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
    public class LoadFromRemote : Base
    {
        [TestMethod]
        public async Task LoadFromRemote_Found_Returns_Assembly()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            var newtonsoftAssemblyFileName = "Newtonsoft.Json.dll";
            var newtonsoftAssemblyPath = Path.Combine(GetPathToAssemblies(), newtonsoftAssemblyFileName);
            var newtonsoftAssembly = Assembly.LoadFile(newtonsoftAssemblyPath);
            var newtonsoftAssemblyName = AssemblyLoadContext.GetAssemblyName(newtonsoftAssemblyPath);
            var newtonsoftAssemblyStream = File.OpenRead(newtonsoftAssemblyPath);
            var newtonsoftAssemblyBytes = File.ReadAllBytes(newtonsoftAssemblyPath);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            fileSystemUtility.Setup(f => f.DoesFileExist($"{GetPathToAssemblies()}/{newtonsoftAssemblyFileName}")).Returns(true);
            fileSystemUtility.Setup(f => f.ReadDependencyFileFromDisk(GetPathToAssemblies(), newtonsoftAssemblyFileName)).Returns(newtonsoftAssemblyStream);
            fileSystemUtility.Setup(f => f.ToByteArray(newtonsoftAssemblyStream)).Returns(newtonsoftAssemblyBytes);

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(GetPluginLoadContext(pluginAssemblyPath));
            var result = loadContext.GetType().GetMethod("LoadFromRemote", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName }) as ValueOrProceed<AssemblyFromStrategy>;

            Assert.IsNotNull(result);
            Assert.AreEqual(newtonsoftAssemblyName.Name, result.Value.Assembly.GetName().Name);
        }

        [TestMethod]
        public async Task LoadFromRemote_NothingFound_Returns_Proceed()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            var newtonsoftAssemblyPath = Path.Combine(GetPathToAssemblies(), "Newtonsoft.Json.dll");
            var newtonsoftAssembly = Assembly.LoadFile(newtonsoftAssemblyPath);
            var newtonsoftAssemblyName = AssemblyLoadContext.GetAssemblyName(newtonsoftAssemblyPath);
            var newtonsoftAssemblyStream = File.OpenRead(newtonsoftAssemblyPath);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            fileSystemUtility.Setup(f => f.DoesFileExist($"{GetPathToAssemblies()}/Newtonsoft.Json.dll")).Returns(false);

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(GetPluginLoadContext(pluginAssemblyPath));
            var result = loadContext.GetType().GetMethod("LoadFromRemote", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName }) as ValueOrProceed<AssemblyFromStrategy>;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.CanProceed);
        }
    }
}