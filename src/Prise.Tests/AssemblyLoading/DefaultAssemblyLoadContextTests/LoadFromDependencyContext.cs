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
    public class LoadFromDependencyContext : Base
    {
        [TestMethod]
        public async Task LoadFromDependencyContext_FromResolver_Works()
        {
            var testContext = SetupAssemblyLoadContext();
            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);

            var newtonsoftAssemblyPath = Path.Combine(GetPathToAssemblies(), "Newtonsoft.Json.dll");
            var newtonsoftAssembly = Assembly.LoadFile(newtonsoftAssemblyPath);
            var newtonsoftAssemblyName = AssemblyLoadContext.GetAssemblyName(newtonsoftAssemblyPath);

            var contract = TestableTypeBuilder.NewTestableType()
               .WithName("IMyTestType")
               .WithNamespace("Test.Type")
               .Build();

            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var resolver = testContext.GetMock<IAssemblyDependencyResolver>();
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            resolver.Setup(r => r.ResolveAssemblyToPath(newtonsoftAssemblyName)).Returns(newtonsoftAssemblyPath);
            fileSystemUtility.Setup(r => r.DoesFileExist(newtonsoftAssemblyPath)).Returns(true);

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(GetPluginLoadContext(pluginAssemblyPath));
            var result = loadContext.GetType().GetMethod("LoadFromDependencyContext", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName }) as ValueOrProceed<AssemblyFromStrategy>;

            Assert.IsNotNull(result);
            Assert.AreEqual(newtonsoftAssemblyName.Name, result.Value.Assembly.GetName().Name);
        }

        [TestMethod]
        public async Task LoadFromDependencyContext_FromResourceAssembly_Returns_Null()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var resolver = testContext.GetMock<IAssemblyDependencyResolver>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            var newtonsoftAssemblyPath = Path.Combine(GetPathToAssemblies(), "Newtonsoft.Json.dll");
            var newtonsoftAssembly = Assembly.LoadFile(newtonsoftAssemblyPath);
            var newtonsoftAssemblyName = AssemblyLoadContext.GetAssemblyName(newtonsoftAssemblyPath);
            // Configure as resource assembly
            newtonsoftAssemblyName.CultureName = "en-GB";

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            resolver.Setup(r => r.ResolveAssemblyToPath(newtonsoftAssemblyName)).Returns(String.Empty);
            pluginDependencyContext.SetupGet(p => p.PluginResourceDependencies).Returns(new List<PluginResourceDependency>());// return empty list

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(GetPluginLoadContext(pluginAssemblyPath));
            var result = loadContext.GetType().GetMethod("LoadFromDependencyContext", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName }) as ValueOrProceed<AssemblyFromStrategy>;

            Assert.IsNotNull(result);
            Assert.IsNull(result.Value);
            Assert.IsFalse(result.CanProceed);
        }

        [TestMethod]
        public async Task LoadFromDependencyContext_FromResourceAssembly_Returns_Assembly()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var resolver = testContext.GetMock<IAssemblyDependencyResolver>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            var newtonsoftAssemblyPath = Path.Combine(GetPathToAssemblies(), "Newtonsoft.Json.dll");
            var newtonsoftAssembly = Assembly.LoadFile(newtonsoftAssemblyPath);
            var newtonsoftAssemblyName = AssemblyLoadContext.GetAssemblyName(newtonsoftAssemblyPath);
            // Configure as resource assembly
            var culture = "en-GB";
            newtonsoftAssemblyName.CultureName = culture;

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

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

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(GetPluginLoadContext(pluginAssemblyPath));
            var result = loadContext.GetType().GetMethod("LoadFromDependencyContext", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName }) as ValueOrProceed<AssemblyFromStrategy>;

            Assert.IsNotNull(result);
            Assert.AreEqual(newtonsoftAssemblyName.Name, result.Value.Assembly.GetName().Name);
        }

        [TestMethod]
        public async Task LoadFromDependencyContext_FromPluginDependency_Returns_Assembly()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var resolver = testContext.GetMock<IAssemblyDependencyResolver>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();
            var pluginDependencyResolver = testContext.GetMock<IPluginDependencyResolver>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            var newtonsoftAssemblyPath = Path.Combine(GetPathToAssemblies(), "Newtonsoft.Json.dll");
            var newtonsoftAssembly = Assembly.LoadFile(newtonsoftAssemblyPath);
            var newtonsoftAssemblyName = AssemblyLoadContext.GetAssemblyName(newtonsoftAssemblyPath);
            var newtonsoftAssemblyStream = File.OpenRead(newtonsoftAssemblyPath);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

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

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(GetPluginLoadContext(pluginAssemblyPath));
            var result = loadContext.GetType().GetMethod("LoadFromDependencyContext", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName }) as ValueOrProceed<AssemblyFromStrategy>;

            Assert.IsNotNull(result);
            Assert.AreEqual(newtonsoftAssemblyName.Name, result.Value.Assembly.GetName().Name);
        }

        [TestMethod]
        public async Task LoadFromDependencyContext_FromLocal_Returns_Assembly()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var resolver = testContext.GetMock<IAssemblyDependencyResolver>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();
            var pluginDependencyResolver = testContext.GetMock<IPluginDependencyResolver>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            var newtonsoftAssemblyPath = Path.Combine(GetPathToAssemblies(), "Newtonsoft.Json.dll");
            var newtonsoftAssembly = Assembly.LoadFile(newtonsoftAssemblyPath);
            var newtonsoftAssemblyName = AssemblyLoadContext.GetAssemblyName(newtonsoftAssemblyPath);
            var newtonsoftAssemblyStream = File.OpenRead(newtonsoftAssemblyPath);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

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

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(GetPluginLoadContext(pluginAssemblyPath));
            var result = loadContext.GetType().GetMethod("LoadFromDependencyContext", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName }) as ValueOrProceed<AssemblyFromStrategy>;

            Assert.IsNotNull(result);
            Assert.AreEqual(newtonsoftAssemblyName.Name, result.Value.Assembly.GetName().Name);
        }

        [TestMethod]
        public async Task LoadFromDependencyContext_NothingFound_Returns_Proceed()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var resolver = testContext.GetMock<IAssemblyDependencyResolver>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();
            var pluginDependencyResolver = testContext.GetMock<IPluginDependencyResolver>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            var newtonsoftAssemblyPath = Path.Combine(GetPathToAssemblies(), "Newtonsoft.Json.dll");
            var newtonsoftAssembly = Assembly.LoadFile(newtonsoftAssemblyPath);
            var newtonsoftAssemblyName = AssemblyLoadContext.GetAssemblyName(newtonsoftAssemblyPath);
            var newtonsoftAssemblyStream = File.OpenRead(newtonsoftAssemblyPath);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

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

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(GetPluginLoadContext(pluginAssemblyPath));
            var result = loadContext.GetType().GetMethod("LoadFromDependencyContext", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName }) as ValueOrProceed<AssemblyFromStrategy>;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.CanProceed);
        }
    }
}