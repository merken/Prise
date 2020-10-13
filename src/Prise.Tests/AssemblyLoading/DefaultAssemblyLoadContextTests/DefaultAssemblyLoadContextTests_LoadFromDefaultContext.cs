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
    public class DefaultAssemblyLoadContextTests_LoadFromDefaultContext : TestWithLoadedPluginBase
    {
        [TestMethod]
        public async Task LoadFromDefaultContext_Returns_Assembly()
        {
            var testContext = await SetupLoadedPluginTextContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var runtimeDefaultAssemblyLoadContext = testContext.GetMock<IRuntimeDefaultAssemblyContext>();
            var initialPluginLoadDirectory = testContext.InitialPluginLoadDirectory;
            var pluginLoadContext = testContext.PluginLoadContext;
            var newtonsoftAssemblyName = testContext.NewtonsoftAssemblyName;
            var newtonsoftAssembly = testContext.NewtonsoftAssembly;

            runtimeDefaultAssemblyLoadContext.Setup(r => r.LoadFromDefaultContext(newtonsoftAssemblyName)).Returns(newtonsoftAssembly);

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
            var testContext = await SetupLoadedPluginTextContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var runtimeDefaultAssemblyLoadContext = testContext.GetMock<IRuntimeDefaultAssemblyContext>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();
            var initialPluginLoadDirectory = testContext.InitialPluginLoadDirectory;
            var pluginLoadContext = testContext.PluginLoadContext;
            var newtonsoftAssemblyName = testContext.NewtonsoftAssemblyName;
            var newtonsoftAssembly = testContext.NewtonsoftAssembly;

            runtimeDefaultAssemblyLoadContext.Setup(r => r.LoadFromDefaultContext(newtonsoftAssemblyName)).Throws<FileNotFoundException>();

            pluginDependencyContext.SetupGet(p => p.HostDependencies).Returns(new[]{
                new HostDependency
                {
                    DependencyName = newtonsoftAssemblyName,
                    AllowDowngrade = false
                }
            });

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
            var testContext = await SetupLoadedPluginTextContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var runtimeDefaultAssemblyLoadContext = testContext.GetMock<IRuntimeDefaultAssemblyContext>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();
            var initialPluginLoadDirectory = testContext.InitialPluginLoadDirectory;
            var pluginLoadContext = testContext.PluginLoadContext;
            var newtonsoftAssemblyName = testContext.NewtonsoftAssemblyName;
            var newtonsoftAssembly = testContext.NewtonsoftAssembly;

            runtimeDefaultAssemblyLoadContext.Setup(r => r.LoadFromDefaultContext(newtonsoftAssemblyName)).Returns((Assembly)null);

            pluginDependencyContext.SetupGet(p => p.HostDependencies).Returns(new[]{
                new HostDependency
                {
                    DependencyName = newtonsoftAssemblyName,
                    AllowDowngrade = true
                }
            });

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
            var testContext = await SetupLoadedPluginTextContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var runtimeDefaultAssemblyLoadContext = testContext.GetMock<IRuntimeDefaultAssemblyContext>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();
            var initialPluginLoadDirectory = testContext.InitialPluginLoadDirectory;
            var pluginLoadContext = testContext.PluginLoadContext;
            var newtonsoftAssemblyName = testContext.NewtonsoftAssemblyName;
            var newtonsoftAssembly = testContext.NewtonsoftAssembly;

            runtimeDefaultAssemblyLoadContext.Setup(r => r.LoadFromDefaultContext(newtonsoftAssemblyName)).Returns((Assembly)null);
            pluginDependencyContext.SetupGet(p => p.HostDependencies).Returns(Enumerable.Empty<HostDependency>());

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
            var testContext = await SetupLoadedPluginTextContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var runtimeDefaultAssemblyLoadContext = testContext.GetMock<IRuntimeDefaultAssemblyContext>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();
            var initialPluginLoadDirectory = testContext.InitialPluginLoadDirectory;
            var pluginLoadContext = testContext.PluginLoadContext;
            var newtonsoftAssemblyName = testContext.NewtonsoftAssemblyName;
            var newtonsoftAssembly = testContext.NewtonsoftAssembly;

            runtimeDefaultAssemblyLoadContext.Setup(r => r.LoadFromDefaultContext(newtonsoftAssemblyName)).Returns((Assembly)null);

            pluginDependencyContext.SetupGet(p => p.HostDependencies).Returns(new[]{
                new HostDependency
                {
                    DependencyName = new AssemblyName()
                }
            });

            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<AssemblyFromStrategy>>(
                loadContext,
                "LoadFromDefaultContext",
                new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName });

            Assert.IsNull(result.Value);
            Assert.IsTrue(result.CanProceed);
        }
    }
}