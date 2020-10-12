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
    public class DefaultAssemblyLoadContextTests_LoadUnmanagedFromDependencyContext : TestWithLoadedPluginBase
    {
        [TestMethod]
        public async Task LoadUnmanagedFromDependencyContext_PreferDependencyContext_Returns_LibraryPath()
        {
            var testContext = await SetupLoadedPluginTextContext((plc) => plc.NativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferDependencyContext);
            var loadContext = testContext.Sut();
            var initialPluginLoadDirectory = testContext.InitialPluginLoadDirectory;
            var resolver = testContext.GetMock<IAssemblyDependencyResolver>();

            var nativeDependency = "Nativelib.dll";
            var libraryPath = "/var/path/to/Nativelib.dll";
            resolver.Setup(r => r.ResolveUnmanagedDllToPath(nativeDependency)).Returns(libraryPath);

            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<string>>(
                loadContext,
                "LoadUnmanagedFromDependencyContext",
                new object[] { initialPluginLoadDirectory, nativeDependency });

            Assert.IsNotNull(result.Value);
            Assert.AreEqual(libraryPath, result.Value);
            Assert.IsFalse(result.CanProceed);
        }

        [TestMethod]
        public async Task LoadUnmanagedFromDependencyContext_PreferInstalledRuntime_Returns_LibraryPath()
        {
            var testContext = await SetupLoadedPluginTextContext((plc) => plc.NativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferInstalledRuntime);
            var loadContext = testContext.Sut();
            var initialPluginLoadDirectory = testContext.InitialPluginLoadDirectory;
            var resolver = testContext.GetMock<IAssemblyDependencyResolver>();
            var pluginDependencyResolver = testContext.GetMock<IPluginDependencyResolver>();

            var nativeDependency = "Nativelib.dll";
            var libraryPath = "/var/path/to/Nativelib.dll";
            var runtimeCandidate = "/var/path/to/runtime/Nativelib.dll";
            resolver.Setup(r => r.ResolveUnmanagedDllToPath(nativeDependency)).Returns(libraryPath);
            pluginDependencyResolver.Setup(r => r.ResolvePlatformDependencyPathToRuntime(It.IsAny<PluginPlatformVersion>(), libraryPath)).Returns(runtimeCandidate);

            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<string>>(
                loadContext,
                "LoadUnmanagedFromDependencyContext",
                new object[] { initialPluginLoadDirectory, nativeDependency });

            Assert.IsNotNull(result.Value);
            Assert.AreEqual(runtimeCandidate, result.Value);
            Assert.IsFalse(result.CanProceed);
        }

        [TestMethod]
        public async Task LoadUnmanagedFromDependencyContext_PreferDependencyContext_Returns_PlatformDependency()
        {
            var testContext = await SetupLoadedPluginTextContext((plc) => plc.NativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferDependencyContext);
            var loadContext = testContext.Sut();
            var initialPluginLoadDirectory = testContext.InitialPluginLoadDirectory;
            var resolver = testContext.GetMock<IAssemblyDependencyResolver>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();
            var pluginDependencyResolver = testContext.GetMock<IPluginDependencyResolver>();

            var nativeDependency = "Nativelib.dll";
            var pathToDependency = "/var/path/to/dependency/Nativelib.dll";
            resolver.Setup(r => r.ResolveUnmanagedDllToPath(nativeDependency)).Returns((string)null);
            var platformDependency = new PlatformDependency
            {
                DependencyNameWithoutExtension = "Nativelib"
            };

            var additionalProbingPaths = Enumerable.Empty<string>();
            pluginDependencyContext.SetupGet(c => c.AdditionalProbingPaths).Returns(additionalProbingPaths);
            pluginDependencyContext.SetupGet(c => c.PlatformDependencies).Returns(new[]{
                platformDependency,
                new PlatformDependency
                {
                    DependencyNameWithoutExtension = "OtherNativelib"
                }
            });
            pluginDependencyResolver
                .Setup(r => r.ResolvePlatformDependencyToPath(initialPluginLoadDirectory, platformDependency, additionalProbingPaths))
                .Returns(pathToDependency);

            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<string>>(
                loadContext,
                "LoadUnmanagedFromDependencyContext",
                new object[] { initialPluginLoadDirectory, nativeDependency });

            Assert.IsNotNull(result.Value);
            Assert.AreEqual(pathToDependency, result.Value);
            Assert.IsFalse(result.CanProceed);
        }

        [TestMethod]
        public async Task LoadUnmanagedFromDependencyContext_PreferInstalledRuntime_Returns_RuntimeCandidate()
        {
            var testContext = await SetupLoadedPluginTextContext((plc) => plc.NativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferInstalledRuntime);
            var loadContext = testContext.Sut();
            var initialPluginLoadDirectory = testContext.InitialPluginLoadDirectory;
            var resolver = testContext.GetMock<IAssemblyDependencyResolver>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();
            var pluginDependencyResolver = testContext.GetMock<IPluginDependencyResolver>();

            var nativeDependency = "Nativelib.dll";
            var pathToDependency = "/var/path/to/dependency/Nativelib.dll";
            var runtimeCandidate = "/var/path/to/runtime/Nativelib.dll";
            resolver.Setup(r => r.ResolveUnmanagedDllToPath(nativeDependency)).Returns((string)null);
            var platformDependency = new PlatformDependency
            {
                DependencyNameWithoutExtension = "Nativelib"
            };
            var additionalProbingPaths = Enumerable.Empty<string>();
            pluginDependencyContext.SetupGet(c => c.AdditionalProbingPaths).Returns(additionalProbingPaths);
            pluginDependencyContext.SetupGet(c => c.PlatformDependencies).Returns(new[]{
                platformDependency,
                new PlatformDependency
                {
                    DependencyNameWithoutExtension = "OtherNativelib"
                }
            });
            pluginDependencyResolver
                .Setup(r => r.ResolvePlatformDependencyToPath(initialPluginLoadDirectory, platformDependency, additionalProbingPaths))
                .Returns(pathToDependency);
            pluginDependencyResolver.Setup(r => r.ResolvePlatformDependencyPathToRuntime(It.IsAny<PluginPlatformVersion>(), pathToDependency)).Returns(runtimeCandidate);

            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<string>>(
                loadContext,
                "LoadUnmanagedFromDependencyContext",
                new object[] { initialPluginLoadDirectory, nativeDependency });

            Assert.IsNotNull(result.Value);
            Assert.AreEqual(runtimeCandidate, result.Value);
            Assert.IsFalse(result.CanProceed);
        }

        [TestMethod]
        public async Task LoadUnmanagedFromDependencyContext_NothingFound_Returns_Empty_And_Proceed()
        {
            var testContext = await SetupLoadedPluginTextContext((plc) => plc.NativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferInstalledRuntime);
            var loadContext = testContext.Sut();
            var initialPluginLoadDirectory = testContext.InitialPluginLoadDirectory;
            var resolver = testContext.GetMock<IAssemblyDependencyResolver>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();

            var nativeDependency = "Nativelib.dll";
            resolver.Setup(r => r.ResolveUnmanagedDllToPath(nativeDependency)).Returns((string)null);
            pluginDependencyContext.SetupGet(c => c.PlatformDependencies).Returns(new[]{
                new PlatformDependency
                {
                    DependencyNameWithoutExtension = "OtherNativelib"
                }
            });

            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<string>>(
                loadContext,
                "LoadUnmanagedFromDependencyContext",
                new object[] { initialPluginLoadDirectory, nativeDependency });

            Assert.AreEqual(String.Empty, result.Value);
            Assert.IsTrue(result.CanProceed);
        }
    }
}