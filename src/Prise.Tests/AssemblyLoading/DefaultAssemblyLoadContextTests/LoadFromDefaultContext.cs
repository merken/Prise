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
    public class LoadFromDefaultContext : Base
    {
        [TestMethod]
        public async Task LoadFromDefaultContext_Returns_Assembly()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var runtimeDefaultAssemblyLoadContext = testContext.GetMock<IRuntimeDefaultAssemblyContext>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            var newtonsoftAssemblyPath = Path.Combine(GetPathToAssemblies(), "Newtonsoft.Json.dll");
            var newtonsoftAssembly = Assembly.LoadFile(newtonsoftAssemblyPath);
            var newtonsoftAssemblyName = AssemblyLoadContext.GetAssemblyName(newtonsoftAssemblyPath);
            var newtonsoftAssemblyStream = File.OpenRead(newtonsoftAssemblyPath);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            runtimeDefaultAssemblyLoadContext.Setup(r => r.LoadFromDefaultContext(newtonsoftAssemblyName)).Returns(newtonsoftAssembly);

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(GetPluginLoadContext(pluginAssemblyPath));
            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<AssemblyFromStrategy>>(
                loadContext,
                "LoadFromDefaultContext",
                new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName });

            Assert.IsNotNull(result);
            Assert.IsFalse(result.CanProceed);
            Assert.IsFalse(result.Value.CanBeReleased);
            Assert.AreEqual(newtonsoftAssemblyName.Name, result.Value.Assembly.GetName().Name);
        }

        [TestMethod]
        public async Task LoadFromDefaultContext_Throws_AssemblyLoadingException_When_FileNotFoundException_And_AllowDowngrade_False()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var runtimeDefaultAssemblyLoadContext = testContext.GetMock<IRuntimeDefaultAssemblyContext>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            var newtonsoftAssemblyPath = Path.Combine(GetPathToAssemblies(), "Newtonsoft.Json.dll");
            var newtonsoftAssembly = Assembly.LoadFile(newtonsoftAssemblyPath);
            var newtonsoftAssemblyName = AssemblyLoadContext.GetAssemblyName(newtonsoftAssemblyPath);
            var newtonsoftAssemblyStream = File.OpenRead(newtonsoftAssemblyPath);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            runtimeDefaultAssemblyLoadContext.Setup(r => r.LoadFromDefaultContext(newtonsoftAssemblyName)).Throws<FileNotFoundException>();

            pluginDependencyContext.SetupGet(p => p.HostDependencies).Returns(new[]{
                new HostDependency
                {
                    DependencyName = newtonsoftAssemblyName,
                    AllowDowngrade = false
                }
            });

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(GetPluginLoadContext(pluginAssemblyPath));
            var exception = Assert
                .ThrowsException<TargetInvocationException>(() =>
                    InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<AssemblyFromStrategy>>(
                        loadContext,
                        "LoadFromDefaultContext",
                        new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName })
                );
            Assert.IsInstanceOfType(exception.InnerException, typeof(AssemblyLoadingException));
        }

        [TestMethod]
        public async Task LoadFromDefaultContext_Returns_Proceed_When_FileNotFoundException_And_AllowDowngrade_True()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var runtimeDefaultAssemblyLoadContext = testContext.GetMock<IRuntimeDefaultAssemblyContext>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            var newtonsoftAssemblyPath = Path.Combine(GetPathToAssemblies(), "Newtonsoft.Json.dll");
            var newtonsoftAssembly = Assembly.LoadFile(newtonsoftAssemblyPath);
            var newtonsoftAssemblyName = AssemblyLoadContext.GetAssemblyName(newtonsoftAssemblyPath);
            var newtonsoftAssemblyStream = File.OpenRead(newtonsoftAssemblyPath);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            runtimeDefaultAssemblyLoadContext.Setup(r => r.LoadFromDefaultContext(newtonsoftAssemblyName)).Returns((Assembly)null);

            pluginDependencyContext.SetupGet(p => p.HostDependencies).Returns(new[]{
                new HostDependency
                {
                    DependencyName = newtonsoftAssemblyName,
                    AllowDowngrade = true
                }
            });

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(GetPluginLoadContext(pluginAssemblyPath));
            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<AssemblyFromStrategy>>(
                loadContext,
                "LoadFromDefaultContext",
                new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName });
            
            Assert.IsNull(result.Value);
            Assert.IsTrue(result.CanProceed);
        }

        [TestMethod]
        public async Task LoadFromDefaultContext_Returns_Proceed_When_Assembly_Null()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var runtimeDefaultAssemblyLoadContext = testContext.GetMock<IRuntimeDefaultAssemblyContext>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            var newtonsoftAssemblyPath = Path.Combine(GetPathToAssemblies(), "Newtonsoft.Json.dll");
            var newtonsoftAssembly = Assembly.LoadFile(newtonsoftAssemblyPath);
            var newtonsoftAssemblyName = AssemblyLoadContext.GetAssemblyName(newtonsoftAssemblyPath);
            var newtonsoftAssemblyStream = File.OpenRead(newtonsoftAssemblyPath);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            runtimeDefaultAssemblyLoadContext.Setup(r => r.LoadFromDefaultContext(newtonsoftAssemblyName)).Returns((Assembly)null);
            pluginDependencyContext.SetupGet(p => p.HostDependencies).Returns(Enumerable.Empty<HostDependency>());

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(GetPluginLoadContext(pluginAssemblyPath));
            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<AssemblyFromStrategy>>(
                loadContext,
                "LoadFromDefaultContext",
                new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName });

            Assert.IsNull(result.Value);
            Assert.IsTrue(result.CanProceed);
        }

        [TestMethod]
        public async Task LoadFromDefaultContext_Returns_Proceed_When_HostAssembly_Not_Found()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var runtimeDefaultAssemblyLoadContext = testContext.GetMock<IRuntimeDefaultAssemblyContext>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            var newtonsoftAssemblyPath = Path.Combine(GetPathToAssemblies(), "Newtonsoft.Json.dll");
            var newtonsoftAssembly = Assembly.LoadFile(newtonsoftAssemblyPath);
            var newtonsoftAssemblyName = AssemblyLoadContext.GetAssemblyName(newtonsoftAssemblyPath);
            var newtonsoftAssemblyStream = File.OpenRead(newtonsoftAssemblyPath);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            runtimeDefaultAssemblyLoadContext.Setup(r => r.LoadFromDefaultContext(newtonsoftAssemblyName)).Returns((Assembly)null);

            pluginDependencyContext.SetupGet(p => p.HostDependencies).Returns(new[]{
                new HostDependency
                {
                    DependencyName = new AssemblyName()
                }
            });

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(GetPluginLoadContext(pluginAssemblyPath));
            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<AssemblyFromStrategy>>(
                loadContext,
                "LoadFromDefaultContext",
                new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName });

            Assert.IsNull(result.Value);
            Assert.IsTrue(result.CanProceed);
        }
    }
}