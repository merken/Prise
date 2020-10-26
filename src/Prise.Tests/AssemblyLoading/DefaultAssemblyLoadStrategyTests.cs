using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prise.AssemblyLoading;

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
    public class DefaultAssemblyLoadStrategyTests : TestBase
    {
        [TestMethod]
        public async Task Ctor_Works()
        {
            Assert.IsNotNull(new DefaultAssemblyLoadStrategy());
        }

        [TestMethod]
        public async Task LoadAssembly_Returns_Null_For_Empty_AssemblyName()
        {
            var sut = new DefaultAssemblyLoadStrategy();
            var emptyAssemblyname = new AssemblyName();
            var initialPluginLoadDirectory = GetPathToAssemblies();

            Assert.IsNull(sut.LoadAssembly(initialPluginLoadDirectory, emptyAssemblyname, null, null, null, null)?.Assembly);
        }

        [TestMethod]
        public async Task LoadAssembly_Returns_Null_When_AssemblyName_NotFound()
        {
            var sut = new DefaultAssemblyLoadStrategy();
            var assemblyname = new AssemblyName("Newtonsoft.Json.dll");
            var pluginDependencyContext = this.mockRepository.Create<IPluginDependencyContext>();
            var loadFromDependencyContext = CreateLookupFunction((c, a) => ValueOrProceed<AssemblyFromStrategy>.Proceed());
            var loadFromRemote = CreateLookupFunction((c, a) => ValueOrProceed<AssemblyFromStrategy>.Proceed());
            var loadFromAppDomain = CreateLookupFunction((c, a) => ValueOrProceed<AssemblyFromStrategy>.Proceed());
            var initialPluginLoadDirectory = GetPathToAssemblies();

            pluginDependencyContext
                .Setup(p => p.HostDependencies).Returns(Enumerable.Empty<HostDependency>());

            pluginDependencyContext
               .Setup(p => p.RemoteDependencies).Returns(Enumerable.Empty<RemoteDependency>());

            Assert.IsNull(sut.LoadAssembly(initialPluginLoadDirectory, assemblyname, pluginDependencyContext.Object, loadFromDependencyContext, loadFromRemote, loadFromAppDomain)?.Assembly);
        }

        [TestMethod]
        public async Task LoadAssembly_Returns_Null_From_AppDomain_When_IsHostAssembly_And_Not_RemoteAssembly()
        {
            var sut = new DefaultAssemblyLoadStrategy();
            var pluginDependencyContext = this.mockRepository.Create<IPluginDependencyContext>();
            var initialPluginLoadDirectory = GetPathToAssemblies();
            var someAssembly = Assembly.LoadFile(Path.Combine(initialPluginLoadDirectory, "Newtonsoft.Json.dll"));
            var someAssemblyName = someAssembly.GetName();
            var loadFromDependencyContext = CreateLookupFunction((c, a) => ValueOrProceed<AssemblyFromStrategy>.Proceed());
            var loadFromRemote = CreateLookupFunction((c, a) => ValueOrProceed<AssemblyFromStrategy>.Proceed());
            var loadFromAppDomain = CreateLookupFunction((c, a) => ValueOrProceed<AssemblyFromStrategy>.FromValue(AssemblyFromStrategy.Releasable(someAssembly), false));

            // Mock the fact that it was setup as a host assembly that should be loaded from the host.
            pluginDependencyContext
                .Setup(p => p.HostDependencies).Returns(new List<HostDependency> { new HostDependency { DependencyName = someAssemblyName } });

            pluginDependencyContext
                .Setup(p => p.RemoteDependencies).Returns(Enumerable.Empty<RemoteDependency>());

            Assert.IsNull(sut.LoadAssembly(initialPluginLoadDirectory, someAssemblyName, pluginDependencyContext.Object, loadFromDependencyContext, loadFromRemote, loadFromAppDomain)?.Assembly);
        }

        [TestMethod]
        public async Task LoadAssembly_Returns_Null_From_AppDomain_When_NOT_IsHostAssembly_And_RemoteAssembly()
        {
            var sut = new DefaultAssemblyLoadStrategy();
            var pluginDependencyContext = this.mockRepository.Create<IPluginDependencyContext>();
            var initialPluginLoadDirectory = GetPathToAssemblies();
            var someAssembly = Assembly.LoadFile(Path.Combine(initialPluginLoadDirectory, "Newtonsoft.Json.dll"));
            var someAssemblyName = someAssembly.GetName();
            var loadFromDependencyContext = CreateLookupFunction((c, a) => ValueOrProceed<AssemblyFromStrategy>.Proceed());
            var loadFromRemote = CreateLookupFunction((c, a) => ValueOrProceed<AssemblyFromStrategy>.Proceed());
            var loadFromAppDomain = CreateLookupFunction((c, a) => ValueOrProceed<AssemblyFromStrategy>.Proceed());

            // Mock the fact that it was setup as a host assembly that should be loaded from the host.
            pluginDependencyContext
                .Setup(p => p.HostDependencies).Returns(new List<HostDependency> { new HostDependency { DependencyName = someAssemblyName } });

            // Mock the fact that it was ALSO setup as a remote assembly that should be loaded from the plugin.
            pluginDependencyContext
                .Setup(p => p.RemoteDependencies).Returns(new List<RemoteDependency> { new RemoteDependency { DependencyName = someAssemblyName } });

            Assert.IsNull(sut.LoadAssembly(initialPluginLoadDirectory, someAssemblyName, pluginDependencyContext.Object, loadFromDependencyContext, loadFromRemote, loadFromAppDomain)?.Assembly);
        }

        [TestMethod]
        public async Task LoadAssembly_Returns_Assembly_From_DependencyContext()
        {
            var sut = new DefaultAssemblyLoadStrategy();
            var pluginDependencyContext = this.mockRepository.Create<IPluginDependencyContext>();
            var initialPluginLoadDirectory = GetPathToAssemblies();
            var someAssembly = Assembly.LoadFile(Path.Combine(initialPluginLoadDirectory, "Newtonsoft.Json.dll"));
            var someAssemblyName = someAssembly.GetName();
            var loadFromDependencyContext = CreateLookupFunction((c, a) => ValueOrProceed<AssemblyFromStrategy>.FromValue(AssemblyFromStrategy.Releasable(someAssembly), false));
            var loadFromRemote = CreateLookupFunction((c, a) => ValueOrProceed<AssemblyFromStrategy>.Proceed());
            var loadFromAppDomain = CreateLookupFunction((c, a) => ValueOrProceed<AssemblyFromStrategy>.Proceed());

            pluginDependencyContext
                .Setup(p => p.HostDependencies).Returns(Enumerable.Empty<HostDependency>());

            pluginDependencyContext
               .Setup(p => p.RemoteDependencies).Returns(Enumerable.Empty<RemoteDependency>());

            Assert.AreEqual(someAssembly, sut.LoadAssembly(initialPluginLoadDirectory, someAssemblyName, pluginDependencyContext.Object, loadFromDependencyContext, loadFromRemote, loadFromAppDomain)?.Assembly);
        }

        [TestMethod]
        public async Task LoadAssembly_Returns_Assembly_From_Remote()
        {
            var sut = new DefaultAssemblyLoadStrategy();
            var pluginDependencyContext = this.mockRepository.Create<IPluginDependencyContext>();
            var initialPluginLoadDirectory = GetPathToAssemblies();
            var someAssembly = Assembly.LoadFile(Path.Combine(initialPluginLoadDirectory, "Newtonsoft.Json.dll"));
            var someAssemblyName = someAssembly.GetName();
            var loadFromDependencyContext = CreateLookupFunction((c, a) => ValueOrProceed<AssemblyFromStrategy>.Proceed());
            var loadFromRemote = CreateLookupFunction((c, a) => ValueOrProceed<AssemblyFromStrategy>.FromValue(AssemblyFromStrategy.Releasable(someAssembly), false));
            var loadFromAppDomain = CreateLookupFunction((c, a) => ValueOrProceed<AssemblyFromStrategy>.Proceed());

            pluginDependencyContext
                .Setup(p => p.HostDependencies).Returns(Enumerable.Empty<HostDependency>());

            pluginDependencyContext
               .Setup(p => p.RemoteDependencies).Returns(Enumerable.Empty<RemoteDependency>());

            Assert.AreEqual(someAssembly, sut.LoadAssembly(initialPluginLoadDirectory, someAssemblyName, pluginDependencyContext.Object, loadFromDependencyContext, loadFromRemote, loadFromAppDomain)?.Assembly);
        }

        [TestMethod]
        public async Task LoadUnmanagedDll_Returns_AssemblyPath_FromDependencyContext()
        {
            var sut = new DefaultAssemblyLoadStrategy();
            var pluginDependencyContext = this.mockRepository.Create<IPluginDependencyContext>();
            var initialPluginLoadDirectory = GetPathToAssemblies();
            var someAssembly = Assembly.LoadFile(Path.Combine(initialPluginLoadDirectory, "Newtonsoft.Json.dll"));
            var someAssemblyName = someAssembly.GetName();
            var loadFromDependencyContext = CreateLookupFunction<string, string>((c, a) => ValueOrProceed<string>.FromValue(someAssemblyName.Name, false));
            var loadFromRemote = CreateLookupFunction<string, string>((c, a) => ValueOrProceed<string>.Proceed());
            var loadFromAppDomain = CreateLookupFunction<string, IntPtr>((c, a) => ValueOrProceed<IntPtr>.FromValue(IntPtr.Zero, false));

            var result = sut.LoadUnmanagedDll(initialPluginLoadDirectory, someAssemblyName.Name, pluginDependencyContext.Object, loadFromDependencyContext, loadFromRemote, loadFromAppDomain);

            Assert.AreEqual(someAssemblyName.Name, result.Path);
            Assert.AreEqual(IntPtr.Zero, result.Pointer);
        }

        [TestMethod]
        public async Task LoadUnmanagedDll_Returns_AssemblyPointer_FromAppDomain()
        {
            var sut = new DefaultAssemblyLoadStrategy();
            var pluginDependencyContext = this.mockRepository.Create<IPluginDependencyContext>();
            var initialPluginLoadDirectory = GetPathToAssemblies();
            var someAssembly = Assembly.LoadFile(Path.Combine(initialPluginLoadDirectory, "Newtonsoft.Json.dll"));
            var someAssemblyName = someAssembly.GetName();
            var loadFromDependencyContext = CreateLookupFunction<string, string>((c, a) => ValueOrProceed<string>.Proceed());
            var loadFromRemote = CreateLookupFunction<string, string>((c, a) => ValueOrProceed<string>.Proceed());
            var loadFromAppDomain = CreateLookupFunction<string, IntPtr>((c, a) => ValueOrProceed<IntPtr>.FromValue(new IntPtr(100), false));

            var result = sut.LoadUnmanagedDll(initialPluginLoadDirectory, someAssemblyName.Name, pluginDependencyContext.Object, loadFromDependencyContext, loadFromRemote, loadFromAppDomain);

            Assert.IsNull(result.Path);
            Assert.AreEqual(new IntPtr(100), result.Pointer);
        }

        [TestMethod]
        public async Task LoadUnmanagedDll_Returns_AssemblyPath_FromRemote()
        {
            var sut = new DefaultAssemblyLoadStrategy();
            var pluginDependencyContext = this.mockRepository.Create<IPluginDependencyContext>();
            var initialPluginLoadDirectory = GetPathToAssemblies();
            var someAssembly = Assembly.LoadFile(Path.Combine(initialPluginLoadDirectory, "Newtonsoft.Json.dll"));
            var someAssemblyName = someAssembly.GetName();
            var loadFromDependencyContext = CreateLookupFunction<string, string>((c, a) => ValueOrProceed<string>.Proceed());
            var loadFromRemote = CreateLookupFunction<string, string>((c, a) => ValueOrProceed<string>.FromValue(someAssemblyName.Name, false));
            var loadFromAppDomain = CreateLookupFunction<string, IntPtr>((c, a) => ValueOrProceed<IntPtr>.Proceed());

            var result = sut.LoadUnmanagedDll(initialPluginLoadDirectory, someAssemblyName.Name, pluginDependencyContext.Object, loadFromDependencyContext, loadFromRemote, loadFromAppDomain);

            Assert.AreEqual(someAssemblyName.Name, result.Path);
            Assert.AreEqual(IntPtr.Zero, result.Pointer);
        }

        protected Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>> CreateLookupFunction(Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>> func) => func;
        protected Func<string, T1, ValueOrProceed<T2>> CreateLookupFunction<T1, T2>(Func<string, T1, ValueOrProceed<T2>> func) => func;
    }
}