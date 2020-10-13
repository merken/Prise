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
    public class DefaultAssemblyLoadContextTests_LoadFromDependencyContext : TestWithLoadedPluginBase
    {
        [TestMethod]
        public async Task LoadFromDependencyContext_FromResolver_Works()
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

            resolver.Setup(r => r.ResolveAssemblyToPath(newtonsoftAssemblyName)).Returns(newtonsoftAssemblyPath);
            fileSystemUtility.Setup(r => r.DoesFileExist(newtonsoftAssemblyPath)).Returns(true);

            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<AssemblyFromStrategy>>(
                            loadContext,
                            "LoadFromDependencyContext",
                            new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName });

            Assert.IsNotNull(result);
            Assert.AreEqual(newtonsoftAssemblyName.Name, result.Value.Assembly.GetName().Name);
        }

        [TestMethod]
        public async Task LoadFromDependencyContext_FromResourceAssembly_Returns_Null()
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

            // Configure as resource assembly
            newtonsoftAssemblyName.CultureName = "en-GB";

            resolver.Setup(r => r.ResolveAssemblyToPath(newtonsoftAssemblyName)).Returns(String.Empty);
            pluginDependencyContext.SetupGet(p => p.PluginResourceDependencies).Returns(new List<PluginResourceDependency>());// return empty list

            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<AssemblyFromStrategy>>(
                            loadContext,
                            "LoadFromDependencyContext",
                            new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName });

            Assert.IsNotNull(result);
            Assert.IsNull(result.Value);
            Assert.IsFalse(result.CanProceed);
        }

        [TestMethod]
        public async Task LoadFromDependencyContext_FromResourceAssembly_Returns_Assembly()
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

            // Configure as resource assembly
            var culture = "en-GB";
            newtonsoftAssemblyName.CultureName = culture;

            resolver.Setup(r => r.ResolveAssemblyToPath(newtonsoftAssemblyName)).Returns(String.Empty);
            pluginDependencyContext.SetupGet(p => p.PluginResourceDependencies).Returns(new[]{
                new PluginResourceDependency
                {
                    Path = GetPathToAssemblies()
                },
                new PluginResourceDependency
                {
                    Path = "/test"
                }
            });

            fileSystemUtility.Setup(r => r.DoesFileExist($"{GetPathToAssemblies()}/{culture}/{newtonsoftAssemblyName.Name}.dll")).Returns(true);

            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<AssemblyFromStrategy>>(
                            loadContext,
                            "LoadFromDependencyContext",
                            new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName });
                            
            Assert.IsNotNull(result);
            Assert.AreEqual(newtonsoftAssemblyName.Name, result.Value.Assembly.GetName().Name);
        }

        [TestMethod]
        public async Task LoadFromDependencyContext_FromPluginDependency_Returns_Assembly()
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
            var pluginDependencyResolver = testContext.GetMock<IPluginDependencyResolver>();

            var newtonsoftAssemblyStream = File.OpenRead(newtonsoftAssemblyPath);

            // Skip the resolver
            resolver.Setup(r => r.ResolveAssemblyToPath(newtonsoftAssemblyName)).Returns(String.Empty);

            // Skip resources assembly
            // newtonsoftAssemblyName does not contain a culture

            var pluginDependency = new PluginDependency
            {
                DependencyNameWithoutExtension = newtonsoftAssemblyName.Name
            };

            var additionalProbingPaths = Enumerable.Empty<string>();
            pluginDependencyContext.SetupGet(p => p.AdditionalProbingPaths).Returns(additionalProbingPaths);
            pluginDependencyContext.SetupGet(p => p.PluginDependencies).Returns(new[]{
                new PluginDependency
                {
                    DependencyNameWithoutExtension = "not-the-droid-im-looking-for"
                },
                pluginDependency
            });

            pluginDependencyResolver.Setup(r => r.ResolvePluginDependencyToPath(initialPluginLoadDirectory, pluginDependency, additionalProbingPaths)).Returns(newtonsoftAssemblyStream);

            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<AssemblyFromStrategy>>(
                            loadContext,
                            "LoadFromDependencyContext",
                            new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName });

            Assert.IsNotNull(result);
            Assert.AreEqual(newtonsoftAssemblyName.Name, result.Value.Assembly.GetName().Name);
        }

        [TestMethod]
        public async Task LoadFromDependencyContext_FromLocal_Returns_Assembly()
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
            var pluginDependencyResolver = testContext.GetMock<IPluginDependencyResolver>();

            // Skip the resolver
            resolver.Setup(r => r.ResolveAssemblyToPath(newtonsoftAssemblyName)).Returns(String.Empty);

            // Skip resources assembly
            // newtonsoftAssemblyName does not contain a culture

            // Skip plugin dependencies
            pluginDependencyContext.SetupGet(p => p.PluginDependencies).Returns(new[]{
                new PluginDependency
                {
                    DependencyNameWithoutExtension = "not-the-droid-im-looking-for"
                }
            });

            // Local file
            fileSystemUtility.Setup(r => r.DoesFileExist($"{GetPathToAssemblies()}/{newtonsoftAssemblyName.Name}.dll")).Returns(true);

            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<AssemblyFromStrategy>>(
                            loadContext,
                            "LoadFromDependencyContext",
                            new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName });

            Assert.IsNotNull(result);
            Assert.AreEqual(newtonsoftAssemblyName.Name, result.Value.Assembly.GetName().Name);
        }

        [TestMethod]
        public async Task LoadFromDependencyContext_NothingFound_Returns_Proceed()
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
            var pluginDependencyResolver = testContext.GetMock<IPluginDependencyResolver>();

            // Skip the resolver
            resolver.Setup(r => r.ResolveAssemblyToPath(newtonsoftAssemblyName)).Returns(String.Empty);

            // Skip resources assembly
            // newtonsoftAssemblyName does not contain a culture

            // Skip plugin dependencies
            pluginDependencyContext.SetupGet(p => p.PluginDependencies).Returns(new[]{
                new PluginDependency
                {
                    DependencyNameWithoutExtension = "not-the-droid-im-looking-for"
                }
            });

            // Local file
            fileSystemUtility.Setup(r => r.DoesFileExist($"{GetPathToAssemblies()}/{newtonsoftAssemblyName.Name}.dll")).Returns(false);

            var result = InvokeProtectedMethodOnLoadContextAndGetResult<ValueOrProceed<AssemblyFromStrategy>>(
                            loadContext,
                            "LoadFromDependencyContext",
                            new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName });
            Assert.IsNotNull(result);
            Assert.IsTrue(result.CanProceed);
        }
    }
}