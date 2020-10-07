using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prise.AssemblyLoading;
using Prise.AssemblyScanning;
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
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(null, null, null, null, null, null));
        }

        [TestMethod]
        public void Ctor_No_DependencyResolverFactory_Throws_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(() => null, null, null, null, null, null));
        }

        [TestMethod]
        public void Ctor_No_AssemblyLoadStrategyFactory_Throws_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(() => null, () => null, null, null, null, null));
        }

        [TestMethod]
        public void Ctor_No_PluginDependencyContextFactory_Throws_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyLoadContext(() => null, () => null, () => null, null, null, null));
        }

        [TestMethod]
        public void Ctor_Works()
        {
            Assert.IsNotNull(new DefaultAssemblyLoadContext(() => null, () => null, () => null, (c) => null, (s) => null, () => null));
        }

        [TestMethod]
        public async Task Load_No_Context_Throws_ArgumentNullException()
        {
            var loadContext = new DefaultAssemblyLoadContext(() => null, () => null, () => null, (c) => null, (s) => null, () => null);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => loadContext.LoadPluginAssembly(null));
        }

        [TestMethod]
        public async Task Load_No_PathToAssembly_Throws_ArgumentNullException()
        {
            var loadContext = new DefaultAssemblyLoadContext(() => null, () => null, () => null, (c) => null, (s) => null, () => null);
            var pluginLoadContext = new PluginLoadContext("Path To Plugin", this.GetType(), "netcoreapp3.1");
            pluginLoadContext.FullPathToPluginAssembly = null;
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => loadContext.LoadPluginAssembly(pluginLoadContext));
        }

        [TestMethod]
        public async Task Load_UnRooted_PathToAssembly_Throws_ArgumentNullException()
        {
            var loadContext = new DefaultAssemblyLoadContext(() => null, () => null, () => null, (c) => null, (s) => null, () => null);
            var pluginLoadContext = new PluginLoadContext("../testpath", this.GetType(), "netcoreapp3.1");
            await Assert.ThrowsExceptionAsync<AssemblyLoadingException>(() => loadContext.LoadPluginAssembly(pluginLoadContext));
        }

        [TestMethod]
        public async Task Load_Works()
        {
            var pluginAssemblyPath = "/var/home/MyPluginAssembly.dll";
            var nativeUnloader = this.mockRepository.Create<INativeAssemblyUnloader>();
            var pluginDependencyResolver = this.mockRepository.Create<IPluginDependencyResolver>();
            var assemblyLoadStrategy = this.mockRepository.Create<IAssemblyLoadStrategy>();
            var pluginDependencyContext = this.mockRepository.Create<IPluginDependencyContext>();
            var resolver = this.mockRepository.Create<IAssemblyDependencyResolver>();
            var fileSystemUtility = this.mockRepository.Create<IFileSystemUtilities>();

            var contract = TestableTypeBuilder.NewTestableType()
                .WithName("IMyTestType")
                .WithNamespace("Test.Type")
                .Build();

            var assembly = this.GetType().Assembly;
            var assemblyStream = File.OpenRead(assembly.Location);

            var loadContext = new DefaultAssemblyLoadContext(
                () => nativeUnloader.Object,
                () => pluginDependencyResolver.Object,
                () => assemblyLoadStrategy.Object,
                (c) => Task.FromResult(pluginDependencyContext.Object),
                (s) => resolver.Object,
                () => fileSystemUtility.Object);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            var pluginLoadContext = new PluginLoadContext(pluginAssemblyPath, contract, "netcoreapp3.1");
            var priseAssembly = await loadContext.LoadPluginAssembly(pluginLoadContext);
            Assert.AreEqual(assembly.FullName, priseAssembly.Assembly.FullName);
        }

        [TestMethod]
        public async Task Load_Guard_Works()
        {
            var pluginAssemblyPath = "/var/home/MyPluginAssembly.dll";
            var nativeUnloader = this.mockRepository.Create<INativeAssemblyUnloader>();
            var pluginDependencyResolver = this.mockRepository.Create<IPluginDependencyResolver>();
            var assemblyLoadStrategy = this.mockRepository.Create<IAssemblyLoadStrategy>();
            var pluginDependencyContext = this.mockRepository.Create<IPluginDependencyContext>();
            var resolver = this.mockRepository.Create<IAssemblyDependencyResolver>();
            var fileSystemUtility = this.mockRepository.Create<IFileSystemUtilities>();

            var contract = TestableTypeBuilder.NewTestableType()
                .WithName("IMyTestType")
                .WithNamespace("Test.Type")
                .Build();

            var assembly = this.GetType().Assembly;
            var assemblyStream = File.OpenRead(assembly.Location);

            var loadContext = new DefaultAssemblyLoadContext(
                () => nativeUnloader.Object,
                () => pluginDependencyResolver.Object,
                () => assemblyLoadStrategy.Object,
                (c) => Task.FromResult(pluginDependencyContext.Object),
                (s) => resolver.Object,
                () => fileSystemUtility.Object);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            var pluginLoadContext = new PluginLoadContext(pluginAssemblyPath, contract, "netcoreapp3.1");
            var priseAssembly = await loadContext.LoadPluginAssembly(pluginLoadContext);

            await Assert.ThrowsExceptionAsync<AssemblyLoadingException>(() => loadContext.LoadPluginAssembly(pluginLoadContext));
        }

        [TestMethod]
        public async Task Disposing_Prevents_Loading()
        {
            var nativeUnloader = this.mockRepository.Create<INativeAssemblyUnloader>();
            var pluginDependencyResolver = this.mockRepository.Create<IPluginDependencyResolver>();
            var assemblyLoadStrategy = this.mockRepository.Create<IAssemblyLoadStrategy>();
            var pluginDependencyContext = this.mockRepository.Create<IPluginDependencyContext>();
            var resolver = this.mockRepository.Create<IAssemblyDependencyResolver>();
            var fileSystemUtility = this.mockRepository.Create<IFileSystemUtilities>();

            var assembly = this.GetType().Assembly;
            var assemblyName = AssemblyLoadContext.GetAssemblyName(assembly.Location);
            var loadContext = new DefaultAssemblyLoadContext(
                () => nativeUnloader.Object,
                () => pluginDependencyResolver.Object,
                () => assemblyLoadStrategy.Object,
                (c) => Task.FromResult(pluginDependencyContext.Object),
                (s) => resolver.Object,
                () => fileSystemUtility.Object);

            loadContext.Dispose();
            var result = loadContext.GetType().GetMethod("Load", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new[] { assemblyName }) as Assembly;
            // var result = loadContext.LoadFromAssemblyPath(assembly.Location);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task Loading_Dependencies_Works()
        {
            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);

            var nativeUnloader = this.mockRepository.Create<INativeAssemblyUnloader>();
            var pluginDependencyResolver = this.mockRepository.Create<IPluginDependencyResolver>();
            var assemblyLoadStrategy = this.mockRepository.Create<IAssemblyLoadStrategy>();
            var pluginDependencyContext = this.mockRepository.Create<IPluginDependencyContext>();
            var resolver = this.mockRepository.Create<IAssemblyDependencyResolver>();
            var fileSystemUtility = this.mockRepository.Create<IFileSystemUtilities>();

            var newtonsoftAssemblyPath = Path.Combine(GetPathToAssemblies(), "Newtonsoft.Json.dll");
            var newtonsoftAssembly = Assembly.LoadFile(newtonsoftAssemblyPath);
            var newtonsoftAssemblyName = AssemblyLoadContext.GetAssemblyName(newtonsoftAssemblyPath);

            var contract = TestableTypeBuilder.NewTestableType()
               .WithName("IMyTestType")
               .WithNamespace("Test.Type")
               .Build();

            var loadContext = new DefaultAssemblyLoadContext(
                () => nativeUnloader.Object,
                () => pluginDependencyResolver.Object,
                () => assemblyLoadStrategy.Object,
                (c) => Task.FromResult(pluginDependencyContext.Object),
                (s) => resolver.Object,
                () => fileSystemUtility.Object);

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

            var pluginLoadContext = new PluginLoadContext(pluginAssemblyPath, contract, "netcoreapp3.1");
            var priseAssembly = await loadContext.LoadPluginAssembly(pluginLoadContext);
            var result = loadContext.GetType().GetMethod("Load", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new[] { newtonsoftAssemblyName }) as Assembly;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Disposing_Prevents_NativeLoading()
        {
            var nativeUnloader = this.mockRepository.Create<INativeAssemblyUnloader>();
            var pluginDependencyResolver = this.mockRepository.Create<IPluginDependencyResolver>();
            var assemblyLoadStrategy = this.mockRepository.Create<IAssemblyLoadStrategy>();
            var pluginDependencyContext = this.mockRepository.Create<IPluginDependencyContext>();
            var resolver = this.mockRepository.Create<IAssemblyDependencyResolver>();
            var fileSystemUtility = this.mockRepository.Create<IFileSystemUtilities>();

            var assemblyName = this.GetType().Assembly.GetName().Name;
            var loadContext = new DefaultAssemblyLoadContext(
                () => nativeUnloader.Object,
                () => pluginDependencyResolver.Object,
                () => assemblyLoadStrategy.Object,
                (c) => Task.FromResult(pluginDependencyContext.Object),
                (s) => resolver.Object,
                () => fileSystemUtility.Object);

            loadContext.Dispose();
            var result = (IntPtr)loadContext.GetType().GetMethod("LoadUnmanagedDll", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new[] { assemblyName });
            Assert.AreEqual(IntPtr.Zero, result);
        }

        [TestMethod]
        public async Task Loading_NativeDependencies_Works()
        {
            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);

            var nativeUnloader = this.mockRepository.Create<INativeAssemblyUnloader>();
            var pluginDependencyResolver = this.mockRepository.Create<IPluginDependencyResolver>();
            var assemblyLoadStrategy = this.mockRepository.Create<IAssemblyLoadStrategy>();
            var pluginDependencyContext = this.mockRepository.Create<IPluginDependencyContext>();
            var resolver = this.mockRepository.Create<IAssemblyDependencyResolver>();
            var fileSystemUtility = this.mockRepository.Create<IFileSystemUtilities>();

            var nativeDependency = "Nativelib.dll";
            var nativePtr = new IntPtr(1024 * 10000);
            var contract = TestableTypeBuilder.NewTestableType()
               .WithName("IMyTestType")
               .WithNamespace("Test.Type")
               .Build();

            var loadContext = new DefaultAssemblyLoadContext(
                () => nativeUnloader.Object,
                () => pluginDependencyResolver.Object,
                () => assemblyLoadStrategy.Object,
                (c) => Task.FromResult(pluginDependencyContext.Object),
                (s) => resolver.Object,
                () => fileSystemUtility.Object);

            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            assemblyLoadStrategy.Setup(a => a.LoadUnmanagedDll(initialPluginLoadDirectory, nativeDependency, pluginDependencyContext.Object,
                It.IsAny<Func<string, string, ValueOrProceed<string>>>(),
                It.IsAny<Func<string, string, ValueOrProceed<string>>>(),
                It.IsAny<Func<string, string, ValueOrProceed<IntPtr>>>())).Returns(NativeAssembly.Create(null, nativePtr));

            var pluginLoadContext = new PluginLoadContext(pluginAssemblyPath, contract, "netcoreapp3.1");
            var priseAssembly = await loadContext.LoadPluginAssembly(pluginLoadContext);
            var result = (IntPtr)loadContext.GetType().GetMethod("LoadUnmanagedDll", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new[] { nativeDependency });
            Assert.IsNotNull(result);
            Assert.AreEqual(nativePtr, result);
        }

        private string GetPathToAssemblies()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assemblies");
        }
    }
}