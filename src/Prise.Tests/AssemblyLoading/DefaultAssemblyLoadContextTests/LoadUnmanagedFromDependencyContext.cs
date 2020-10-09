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
    public class LoadUnmanagedFromDependencyContext : Base
    {
        [TestMethod]
        public async Task LoadUnmanagedFromDependencyContext_PreferDependencyContext_Returns_LibraryPath()
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

            var nativeDependency = "Nativelib.dll";
            var libraryPath = "/var/path/to/Nativelib.dll";
            dependencyResolver.Setup(r => r.ResolveUnmanagedDllToPath(nativeDependency)).Returns(libraryPath);

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(
                GetPluginLoadContext(pluginAssemblyPath,
                (plc) => plc.NativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferDependencyContext));
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
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var runtimeDefaultAssemblyLoadContext = testContext.GetMock<IRuntimeDefaultAssemblyContext>();
            var dependencyResolver = testContext.GetMock<IAssemblyDependencyResolver>();
            var pluginDependencyResolver = testContext.GetMock<IPluginDependencyResolver>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            var nativeDependency = "Nativelib.dll";
            var libraryPath = "/var/path/to/Nativelib.dll";
            var runtimeCandidate = "/var/path/to/runtime/Nativelib.dll";
            dependencyResolver.Setup(r => r.ResolveUnmanagedDllToPath(nativeDependency)).Returns(libraryPath);
            pluginDependencyResolver.Setup(r => r.ResolvePlatformDependencyPathToRuntime(It.IsAny<PluginPlatformVersion>(), libraryPath)).Returns(runtimeCandidate);

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(
                GetPluginLoadContext(pluginAssemblyPath,
                (plc) => plc.NativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferInstalledRuntime));
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
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var runtimeDefaultAssemblyLoadContext = testContext.GetMock<IRuntimeDefaultAssemblyContext>();
            var dependencyResolver = testContext.GetMock<IAssemblyDependencyResolver>();
            var pluginDependencyResolver = testContext.GetMock<IPluginDependencyResolver>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            var nativeDependency = "Nativelib.dll";
            var pathToDependency = "/var/path/to/dependency/Nativelib.dll";
            dependencyResolver.Setup(r => r.ResolveUnmanagedDllToPath(nativeDependency)).Returns((string)null);
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

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(
                GetPluginLoadContext(pluginAssemblyPath,
                (plc) => plc.NativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferDependencyContext));
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
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var runtimeDefaultAssemblyLoadContext = testContext.GetMock<IRuntimeDefaultAssemblyContext>();
            var dependencyResolver = testContext.GetMock<IAssemblyDependencyResolver>();
            var pluginDependencyResolver = testContext.GetMock<IPluginDependencyResolver>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            var nativeDependency = "Nativelib.dll";
            var pathToDependency = "/var/path/to/dependency/Nativelib.dll";
            var runtimeCandidate = "/var/path/to/runtime/Nativelib.dll";
            dependencyResolver.Setup(r => r.ResolveUnmanagedDllToPath(nativeDependency)).Returns((string)null);
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

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(
                GetPluginLoadContext(pluginAssemblyPath,
                (plc) => plc.NativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferInstalledRuntime));
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
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var runtimeDefaultAssemblyLoadContext = testContext.GetMock<IRuntimeDefaultAssemblyContext>();
            var dependencyResolver = testContext.GetMock<IAssemblyDependencyResolver>();
            var pluginDependencyResolver = testContext.GetMock<IPluginDependencyResolver>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            var nativeDependency = "Nativelib.dll";
            dependencyResolver.Setup(r => r.ResolveUnmanagedDllToPath(nativeDependency)).Returns((string)null);
            pluginDependencyContext.SetupGet(c => c.PlatformDependencies).Returns(new[]{
                new PlatformDependency
                {
                    DependencyNameWithoutExtension = "OtherNativelib"
                }
            });

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(
                GetPluginLoadContext(pluginAssemblyPath,
                (plc) => plc.NativeDependencyLoadPreference = NativeDependencyLoadPreference.PreferInstalledRuntime));
            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<string>>(
                loadContext,
                "LoadUnmanagedFromDependencyContext",
                new object[] { initialPluginLoadDirectory, nativeDependency });

            Assert.AreEqual(String.Empty, result.Value);
            Assert.IsTrue(result.CanProceed);
        }
    }
}