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

namespace Prise.Tests.AssemblyLoading
{
    [TestClass]
    public class DefaultAssemblyLoadContextTests : TestBase
    {
        [TestMethod]
        public void Ctor_No_NativeAssemblyUnloaderFactory_Throws_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(null, null, null, null, null, null, null));
        }

        [TestMethod]
        public void Ctor_No_DependencyResolverFactory_Throws_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(() => null, null, null, null, null, null, null));
        }

        [TestMethod]
        public void Ctor_No_AssemblyLoadStrategyFactory_Throws_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(() => null, () => null, null, null, null, null, null));
        }

        [TestMethod]
        public void Ctor_No_PluginDependencyContextFactory_Throws_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(() => null, () => null, () => null, null, null, null, null));
        }


        [TestMethod]
        public void Ctor_No_FileSystemUtilitiesFactory_Throws_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(() => null, () => null, () => null, (c) => null, (s) => null, () => null, null));
        }


        [TestMethod]
        public void Ctor_Works()
        {
            Assert.IsNotNull(new DefaultAssemblyLoadContext(() => null, () => null, () => null, (c) => null, (s) => null, () => null, () => null));
        }

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

            var contract = TestableTypeBuilder.NewTestableType()
                .WithName("IMyTestType")
                .WithNamespace("Test.Type")
                .Build();

            var assembly = this.GetType().Assembly;
            var assemblyStream = File.OpenRead(assembly.Location);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            var pluginLoadContext = new PluginLoadContext(pluginAssemblyPath, contract, "netcoreapp3.1");
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

            var contract = TestableTypeBuilder.NewTestableType()
                .WithName("IMyTestType")
                .WithNamespace("Test.Type")
                .Build();

            var assembly = this.GetType().Assembly;
            var assemblyStream = File.OpenRead(assembly.Location);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            var pluginLoadContext = new PluginLoadContext(pluginAssemblyPath, contract, "netcoreapp3.1");
            var priseAssembly = await loadContext.LoadPluginAssembly(pluginLoadContext);

            await Assert.ThrowsExceptionAsync<AssemblyLoadingException>(() => loadContext.LoadPluginAssembly(pluginLoadContext));
        }

        [TestMethod]
        public async Task Disposing_Prevents_Load()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();

            var assembly = this.GetType().Assembly;
            var assemblyName = AssemblyLoadContext.GetAssemblyName(assembly.Location);

            loadContext.Dispose();
            var result = loadContext.GetType().GetMethod("Load", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new[] { assemblyName }) as Assembly;
            // var result = loadContext.LoadFromAssemblyPath(assembly.Location);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task Load_Works()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var assemblyLoadStrategy = testContext.GetMock<IAssemblyLoadStrategy>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);

            var newtonsoftAssemblyPath = Path.Combine(GetPathToAssemblies(), "Newtonsoft.Json.dll");
            var newtonsoftAssembly = Assembly.LoadFile(newtonsoftAssemblyPath);
            var newtonsoftAssemblyName = AssemblyLoadContext.GetAssemblyName(newtonsoftAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            assemblyLoadStrategy.Setup(a => a.LoadAssembly(initialPluginLoadDirectory, newtonsoftAssemblyName, pluginDependencyContext.Object,
                It.IsAny<Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>>>(),
                It.IsAny<Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>>>(),
                It.IsAny<Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>>>())).Returns(new AssemblyFromStrategy
                {
                    Assembly = newtonsoftAssembly,
                    CanBeReleased = true
                });

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(GetPluginLoadContext(pluginAssemblyPath));
            var result = loadContext.GetType().GetMethod("Load", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new[] { newtonsoftAssemblyName }) as Assembly;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Disposing_Prevents_LoadUnmanagedDll()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();

            var assemblyName = this.GetType().Assembly.GetName().Name;
            loadContext.Dispose();

            var result = (IntPtr)loadContext.GetType().GetMethod("LoadUnmanagedDll", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new[] { assemblyName });
            Assert.AreEqual(IntPtr.Zero, result);
        }

        [TestMethod]
        public async Task LoadUnmanagedDll_Works()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var assemblyLoadStrategy = testContext.GetMock<IAssemblyLoadStrategy>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            var nativeDependency = "Nativelib.dll";
            var nativePtr = new IntPtr(1024 * 10000);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            assemblyLoadStrategy.Setup(a => a.LoadUnmanagedDll(initialPluginLoadDirectory, nativeDependency, pluginDependencyContext.Object,
                It.IsAny<Func<string, string, ValueOrProceed<string>>>(),
                It.IsAny<Func<string, string, ValueOrProceed<string>>>(),
                It.IsAny<Func<string, string, ValueOrProceed<IntPtr>>>())).Returns(NativeAssembly.Create(null, nativePtr));

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(GetPluginLoadContext(pluginAssemblyPath));
            var result = (IntPtr)loadContext.GetType().GetMethod("LoadUnmanagedDll", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new[] { nativeDependency });

            Assert.IsNotNull(result);
            Assert.AreEqual(nativePtr, result);
        }

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

        // LoadFromDefaultContext

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
            var result = loadContext.GetType().GetMethod("LoadFromDefaultContext", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName }) as ValueOrProceed<AssemblyFromStrategy>;

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
            var exception = Assert.ThrowsException<TargetInvocationException>(() => loadContext.GetType().GetMethod("LoadFromDefaultContext", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName }));
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
            var result = loadContext.GetType().GetMethod("LoadFromDefaultContext", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName }) as ValueOrProceed<AssemblyFromStrategy>;

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
            var result = loadContext.GetType().GetMethod("LoadFromDefaultContext", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName }) as ValueOrProceed<AssemblyFromStrategy>;

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
            var result = loadContext.GetType().GetMethod("LoadFromDefaultContext", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new object[] { initialPluginLoadDirectory, newtonsoftAssemblyName }) as ValueOrProceed<AssemblyFromStrategy>;

            Assert.IsNull(result.Value);
            Assert.IsTrue(result.CanProceed);
        }

        private IPluginLoadContext GetPluginLoadContext(string pluginAssemblyPath) => new PluginLoadContext(pluginAssemblyPath, GetContractType(), "netcoreapp3.1");

        private Type GetContractType()
        {
            return TestableTypeBuilder.NewTestableType()
               .WithName("IMyTestType")
               .WithNamespace("Test.Type")
               .Build();
        }

        private TestContext<IAssemblyLoadContext> SetupAssemblyLoadContext()
        {
            var nativeUnloader = this.mockRepository.Create<INativeAssemblyUnloader>();
            var pluginDependencyResolver = this.mockRepository.Create<IPluginDependencyResolver>();
            var assemblyLoadStrategy = this.mockRepository.Create<IAssemblyLoadStrategy>();
            var pluginDependencyContext = this.mockRepository.Create<IPluginDependencyContext>();
            var resolver = this.mockRepository.Create<IAssemblyDependencyResolver>();
            var fileSystemUtility = this.mockRepository.Create<IFileSystemUtilities>();
            var runtimeDefaultAssemblyLoadContext = this.mockRepository.Create<IRuntimeDefaultAssemblyContext>();

            var loadContext = new DefaultAssemblyLoadContext(
               () => nativeUnloader.Object,
               () => pluginDependencyResolver.Object,
               () => assemblyLoadStrategy.Object,
               (c) => Task.FromResult(pluginDependencyContext.Object),
               (s) => resolver.Object,
               () => fileSystemUtility.Object,
               () => runtimeDefaultAssemblyLoadContext.Object);

            return new TestContext<IAssemblyLoadContext>(loadContext,
                nativeUnloader,
                pluginDependencyResolver,
                assemblyLoadStrategy,
                pluginDependencyContext,
                resolver,
                fileSystemUtility,
                runtimeDefaultAssemblyLoadContext
            );
        }

        private string GetPathToAssemblies()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assemblies");
        }
    }
}