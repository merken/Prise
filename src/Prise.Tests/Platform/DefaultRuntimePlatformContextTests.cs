using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prise.AssemblyScanning;
using Prise.Caching;

using Prise.Platform;
using Prise.Tests.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise.Tests.Platform
{
    [TestClass]
    public class DefaultRuntimePlatformContextTests : TestBase
    {
        [TestMethod]
        public void Ctor_Throws_When_PlatformAbstraction_Null()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new DefaultRuntimePlatformContext(null, null));
            exception.Message.Contains("platformAbstractionFactory");
        }

        [TestMethod]
        public void Ctor_Throws_When_DirectoryTraverser_Null()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new DefaultRuntimePlatformContext(() => null, null));
            exception.Message.Contains("directoryTraverserFactory");
        }

        [TestMethod]
        public void Ctor_Works()
        {
            Assert.IsNotNull(new DefaultRuntimePlatformContext(() => null, () => null));
        }

        [TestMethod]
        public void GetPlatformExtensions_NotSupported_Throws()
        {
            var platformAbstraction = this.mockRepository.Create<IPlatformAbstraction>();
            var directoryTraverser = this.mockRepository.Create<IDirectoryTraverser>();

            platformAbstraction.Setup(p => p.IsWindows()).Returns(false);
            platformAbstraction.Setup(p => p.IsLinux()).Returns(false);
            platformAbstraction.Setup(p => p.IsOSX()).Returns(false);

            var context = new DefaultRuntimePlatformContext(() => platformAbstraction.Object, () => directoryTraverser.Object);

            Assert.ThrowsException<PlatformException>(() => context.GetPlatformExtensions());
        }

        [TestMethod]
        public void GetPlatformExtensions_Linux_Works()
        {
            var platformAbstraction = this.mockRepository.Create<IPlatformAbstraction>();
            var directoryTraverser = this.mockRepository.Create<IDirectoryTraverser>();

            platformAbstraction.Setup(p => p.IsWindows()).Returns(false);
            platformAbstraction.Setup(p => p.IsLinux()).Returns(true);

            var context = new DefaultRuntimePlatformContext(() => platformAbstraction.Object, () => directoryTraverser.Object);
            var results = context.GetPlatformExtensions();
            Assert.AreEqual(2, results.Count());
            Assert.AreEqual(".so", results.First());
            Assert.AreEqual(".so.1", results.ElementAt(1));
        }

        [TestMethod]
        public void GetPlatformExtensions_Windows_Works()
        {
            var platformAbstraction = this.mockRepository.Create<IPlatformAbstraction>();
            var directoryTraverser = this.mockRepository.Create<IDirectoryTraverser>();

            platformAbstraction.Setup(p => p.IsWindows()).Returns(true);

            var context = new DefaultRuntimePlatformContext(() => platformAbstraction.Object, () => directoryTraverser.Object);
            var results = context.GetPlatformExtensions();
            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(".dll", results.First());
        }

        [TestMethod]
        public void GetPlatformExtensions_OSX_Works()
        {
            var platformAbstraction = this.mockRepository.Create<IPlatformAbstraction>();
            var directoryTraverser = this.mockRepository.Create<IDirectoryTraverser>();

            platformAbstraction.Setup(p => p.IsWindows()).Returns(false);
            platformAbstraction.Setup(p => p.IsLinux()).Returns(false);
            platformAbstraction.Setup(p => p.IsOSX()).Returns(true);

            var context = new DefaultRuntimePlatformContext(() => platformAbstraction.Object, () => directoryTraverser.Object);
            var results = context.GetPlatformExtensions();
            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(".dylib", results.First());
        }

        [TestMethod]
        public void GetPluginDependencyNames_Works()
        {
            var platformAbstraction = this.mockRepository.Create<IPlatformAbstraction>();
            var directoryTraverser = this.mockRepository.Create<IDirectoryTraverser>();
            var context = new DefaultRuntimePlatformContext(() => platformAbstraction.Object, () => directoryTraverser.Object);

            var results = context.GetPluginDependencyNames("MyPlugin");
            Assert.AreEqual(4, results.Count());
            Assert.AreEqual("MyPlugin.dll", results.First());
            Assert.AreEqual("MyPlugin.ni.dll", results.ElementAt(1));
            Assert.AreEqual("MyPlugin.exe", results.ElementAt(2));
            Assert.AreEqual("MyPlugin.ni.exe", results.ElementAt(3));
        }

        [TestMethod]
        public void GetPlatformDependencyNames_NotSupported_Throws()
        {
            var platformAbstraction = this.mockRepository.Create<IPlatformAbstraction>();
            var directoryTraverser = this.mockRepository.Create<IDirectoryTraverser>();

            platformAbstraction.Setup(p => p.IsWindows()).Returns(false);
            platformAbstraction.Setup(p => p.IsLinux()).Returns(false);
            platformAbstraction.Setup(p => p.IsOSX()).Returns(false);

            var context = new DefaultRuntimePlatformContext(() => platformAbstraction.Object, () => directoryTraverser.Object);

            Assert.ThrowsException<PlatformException>(() => context.GetPlatformDependencyNames("MyPlugin"));
        }

        [TestMethod]
        public void GetPlatformDependencyNames_Linux_Works()
        {
            var platformAbstraction = this.mockRepository.Create<IPlatformAbstraction>();
            var directoryTraverser = this.mockRepository.Create<IDirectoryTraverser>();

            platformAbstraction.Setup(p => p.IsWindows()).Returns(false);
            platformAbstraction.Setup(p => p.IsLinux()).Returns(true);

            var context = new DefaultRuntimePlatformContext(() => platformAbstraction.Object, () => directoryTraverser.Object);
            var results = context.GetPlatformDependencyNames("MyPlugin");
            Assert.AreEqual(4, results.Count());
            Assert.AreEqual("MyPlugin.so", results.First());
            Assert.AreEqual("MyPlugin.so.1", results.ElementAt(1));
            Assert.AreEqual("libMyPlugin.so", results.ElementAt(2));
            Assert.AreEqual("libMyPlugin.so.1", results.ElementAt(3));
        }

        [TestMethod]
        public void GetPlatformDependencyNames_Windows_Works()
        {
            var platformAbstraction = this.mockRepository.Create<IPlatformAbstraction>();
            var directoryTraverser = this.mockRepository.Create<IDirectoryTraverser>();

            platformAbstraction.Setup(p => p.IsWindows()).Returns(true);

            var context = new DefaultRuntimePlatformContext(() => platformAbstraction.Object, () => directoryTraverser.Object);
            var results = context.GetPlatformDependencyNames("MyPlugin");
            Assert.AreEqual(1, results.Count());
            Assert.AreEqual("MyPlugin.dll", results.First());
        }

        [TestMethod]
        public void GetPlatformDependencyNames_OSX_Works()
        {
            var platformAbstraction = this.mockRepository.Create<IPlatformAbstraction>();
            var directoryTraverser = this.mockRepository.Create<IDirectoryTraverser>();

            platformAbstraction.Setup(p => p.IsWindows()).Returns(false);
            platformAbstraction.Setup(p => p.IsLinux()).Returns(false);
            platformAbstraction.Setup(p => p.IsOSX()).Returns(true);

            var context = new DefaultRuntimePlatformContext(() => platformAbstraction.Object, () => directoryTraverser.Object);
            var results = context.GetPlatformDependencyNames("MyPlugin");
            Assert.AreEqual(2, results.Count());
            Assert.AreEqual("MyPlugin.dylib", results.First());
            Assert.AreEqual("libMyPlugin.dylib", results.ElementAt(1));
        }


        [TestMethod]
        public void GetRuntimeInfo_NotSupported_Throws()
        {
            var platformAbstraction = this.mockRepository.Create<IPlatformAbstraction>();
            var directoryTraverser = this.mockRepository.Create<IDirectoryTraverser>();

            platformAbstraction.Setup(p => p.IsWindows()).Returns(false);
            platformAbstraction.Setup(p => p.IsLinux()).Returns(false);
            platformAbstraction.Setup(p => p.IsOSX()).Returns(false);

            var context = new DefaultRuntimePlatformContext(() => platformAbstraction.Object, () => directoryTraverser.Object);

            Assert.ThrowsException<PlatformException>(() => context.GetRuntimeInfo());
        }

        [TestMethod]
        public void GetRuntimeInfo_Unknown_Runtime_Throws()
        {
            var platformAbstraction = this.mockRepository.Create<IPlatformAbstraction>();
            var directoryTraverser = this.mockRepository.Create<IDirectoryTraverser>();

            var unknownPlatorm = "MICROSOFT.ASPNETNEW.APP";
            var platormDependendLocation = "/usr/share/dotnet/shared";

            platformAbstraction.Setup(p => p.IsWindows()).Returns(false);
            platformAbstraction.Setup(p => p.IsLinux()).Returns(true);
            directoryTraverser.Setup(d => d.TraverseDirectories(platormDependendLocation)).Returns(new[] { $"{platormDependendLocation}/{unknownPlatorm}" });

            var context = new DefaultRuntimePlatformContext(() => platformAbstraction.Object, () => directoryTraverser.Object);

            Assert.ThrowsException<PlatformException>(() => context.GetRuntimeInfo());
        }

        [TestMethod]
        public void GetRuntimeInfo_Linux_Works()
        {
            var platormDependendLocation = "/usr/share/dotnet/shared";
            var runtimes = new[]{
                $"{platormDependendLocation}/MICROSOFT.ASPNETCORE.ALL",
                $"{platormDependendLocation}/MICROSOFT.ASPNETCORE.APP",
                $"{platormDependendLocation}/MICROSOFT.NETCORE.APP",
            };
            var versions = new[]{
                $"2.1.0",
                $"3.1.0",
                $"5.0.0"
            };
            var platformAbstraction = this.mockRepository.Create<IPlatformAbstraction>();
            var directoryTraverser = this.mockRepository.Create<IDirectoryTraverser>();

            platformAbstraction.Setup(p => p.IsWindows()).Returns(false);
            platformAbstraction.Setup(p => p.IsLinux()).Returns(true);
            directoryTraverser.Setup(d => d.TraverseDirectories(platormDependendLocation)).Returns(runtimes);
            directoryTraverser.Setup(d => d.TraverseDirectories(runtimes[0])).Returns(versions.Select(v => $"{runtimes[0]}/{v}"));
            directoryTraverser.Setup(d => d.TraverseDirectories(runtimes[1])).Returns(versions.Select(v => $"{runtimes[1]}/{v}"));
            directoryTraverser.Setup(d => d.TraverseDirectories(runtimes[2])).Returns(versions.Select(v => $"{runtimes[2]}/{v}"));

            var context = new DefaultRuntimePlatformContext(() => platformAbstraction.Object, () => directoryTraverser.Object);
            var result = context.GetRuntimeInfo();

            Assert.AreEqual(9, result.Runtimes.Count());
            Assert.AreEqual("2.1.0", result.Runtimes.ElementAt(0).Version);
            Assert.AreEqual("3.1.0", result.Runtimes.ElementAt(1).Version);
            Assert.AreEqual("5.0.0", result.Runtimes.ElementAt(2).Version);
            Assert.AreEqual(RuntimeType.AspNetCoreAll, result.Runtimes.ElementAt(0).RuntimeType);
            Assert.AreEqual(RuntimeType.AspNetCoreApp, result.Runtimes.ElementAt(3).RuntimeType);
            Assert.AreEqual(RuntimeType.NetCoreApp, result.Runtimes.ElementAt(6).RuntimeType);
        }

        [TestMethod]
        public void GetRuntimeInfo_Windows_Works()
        {
            var platormDependendLocation = "C:\\Program Files\\dotnet\\shared";
            var runtimes = new[]{
                $"{platormDependendLocation}/MICROSOFT.ASPNETCORE.ALL",
                $"{platormDependendLocation}/MICROSOFT.ASPNETCORE.APP",
                $"{platormDependendLocation}/MICROSOFT.NETCORE.APP",
                $"{platormDependendLocation}/MICROSOFT.WINDOWSDESKTOP.APP",
            };
            var versions = new[]{
                $"2.1.0",
                $"3.1.0",
                $"5.0.0"
            };
            var platformAbstraction = this.mockRepository.Create<IPlatformAbstraction>();
            var directoryTraverser = this.mockRepository.Create<IDirectoryTraverser>();

            platformAbstraction.Setup(p => p.IsWindows()).Returns(true);
            directoryTraverser.Setup(d => d.TraverseDirectories(System.IO.Path.GetFullPath(platormDependendLocation))).Returns(runtimes);
            directoryTraverser.Setup(d => d.TraverseDirectories(runtimes[0])).Returns(versions.Select(v => $"{runtimes[0]}/{v}"));
            directoryTraverser.Setup(d => d.TraverseDirectories(runtimes[1])).Returns(versions.Select(v => $"{runtimes[1]}/{v}"));
            directoryTraverser.Setup(d => d.TraverseDirectories(runtimes[2])).Returns(versions.Select(v => $"{runtimes[2]}/{v}"));
            directoryTraverser.Setup(d => d.TraverseDirectories(runtimes[3])).Returns(versions.Select(v => $"{runtimes[3]}/{v}"));

            var context = new DefaultRuntimePlatformContext(() => platformAbstraction.Object, () => directoryTraverser.Object);
            var result = context.GetRuntimeInfo();

            Assert.AreEqual(12, result.Runtimes.Count());
            Assert.AreEqual("2.1.0", result.Runtimes.ElementAt(0).Version);
            Assert.AreEqual("3.1.0", result.Runtimes.ElementAt(1).Version);
            Assert.AreEqual("5.0.0", result.Runtimes.ElementAt(2).Version);
            Assert.AreEqual(RuntimeType.AspNetCoreAll, result.Runtimes.ElementAt(0).RuntimeType);
            Assert.AreEqual(RuntimeType.AspNetCoreApp, result.Runtimes.ElementAt(3).RuntimeType);
            Assert.AreEqual(RuntimeType.NetCoreApp, result.Runtimes.ElementAt(6).RuntimeType);
            Assert.AreEqual(RuntimeType.WindowsDesktopApp, result.Runtimes.ElementAt(9).RuntimeType);
        }

        [TestMethod]
        public void GetRuntimeInfo_OSX_Works()
        {
            var platormDependendLocation = "/usr/local/share/dotnet/shared";
            var runtimes = new[]{
                $"{platormDependendLocation}/MICROSOFT.ASPNETCORE.ALL",
                $"{platormDependendLocation}/MICROSOFT.ASPNETCORE.APP",
                $"{platormDependendLocation}/MICROSOFT.NETCORE.APP",
            };
            var versions = new[]{
                $"2.1.0",
                $"3.1.0",
                $"5.0.0"
            };
            var platformAbstraction = this.mockRepository.Create<IPlatformAbstraction>();
            var directoryTraverser = this.mockRepository.Create<IDirectoryTraverser>();

            platformAbstraction.Setup(p => p.IsWindows()).Returns(false);
            platformAbstraction.Setup(p => p.IsLinux()).Returns(false);
            platformAbstraction.Setup(p => p.IsOSX()).Returns(true);
            directoryTraverser.Setup(d => d.TraverseDirectories(platormDependendLocation)).Returns(runtimes);
            directoryTraverser.Setup(d => d.TraverseDirectories(runtimes[0])).Returns(versions.Select(v => $"{runtimes[0]}/{v}"));
            directoryTraverser.Setup(d => d.TraverseDirectories(runtimes[1])).Returns(versions.Select(v => $"{runtimes[1]}/{v}"));
            directoryTraverser.Setup(d => d.TraverseDirectories(runtimes[2])).Returns(versions.Select(v => $"{runtimes[2]}/{v}"));

            var context = new DefaultRuntimePlatformContext(() => platformAbstraction.Object, () => directoryTraverser.Object);
            var result = context.GetRuntimeInfo();

            Assert.AreEqual(9, result.Runtimes.Count());
            Assert.AreEqual("2.1.0", result.Runtimes.ElementAt(0).Version);
            Assert.AreEqual("3.1.0", result.Runtimes.ElementAt(1).Version);
            Assert.AreEqual("5.0.0", result.Runtimes.ElementAt(2).Version);
            Assert.AreEqual(RuntimeType.AspNetCoreAll, result.Runtimes.ElementAt(0).RuntimeType);
            Assert.AreEqual(RuntimeType.AspNetCoreApp, result.Runtimes.ElementAt(3).RuntimeType);
            Assert.AreEqual(RuntimeType.NetCoreApp, result.Runtimes.ElementAt(6).RuntimeType);
        }
    }
}