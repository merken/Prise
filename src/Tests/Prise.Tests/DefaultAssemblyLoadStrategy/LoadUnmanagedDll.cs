using Prise.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace Prise.Tests.DefaultAssemblyLoadStrategy
{
    public class LoadUnmanagedDll : DefaultAssemblyLoadStrategyBase
    {
        [Fact]
        public void Returns_AssemblyPath_FromDependencyContext()
        {
            // Arrange
            var someAssembly = GetRealAssembly();
            var someAssemblyName = someAssembly.GetName();
            var pluginLogger = this.LooseMock<IPluginLogger>();
            var pluginLoadContext = this.Mock<IPluginLoadContext>();
            var pluginDependencyContext = this.Mock<IPluginDependencyContext>();
            var sut = new Prise.DefaultAssemblyLoadStrategy(pluginLogger, pluginLoadContext, pluginDependencyContext);
            var loadFromDependencyContext = CreateLookupFunction<string, string>((c, a) => ValueOrProceed<string>.FromValue(someAssemblyName.Name, false));
            var loadFromRemote = CreateLookupFunction<string, string>((c, a) => ValueOrProceed<string>.Proceed());
            var loadFromAppDomain = CreateLookupFunction<string, IntPtr>((c, a) => ValueOrProceed<IntPtr>.FromValue(IntPtr.Zero, false));

            // Act
            var result = sut.LoadUnmanagedDll(someAssemblyName.Name, loadFromDependencyContext, loadFromRemote, loadFromAppDomain);

            // Assert
            Assert.Equal(someAssemblyName.Name, result.Path);
            Assert.Equal(IntPtr.Zero, result.Pointer);
        }

        [Fact]
        public void Returns_AssemblyPointer_FromAppDomain()
        {
            // Arrange
            var someAssembly = GetRealAssembly();
            var someAssemblyName = someAssembly.GetName();
            var pluginLogger = this.LooseMock<IPluginLogger>();
            var pluginLoadContext = this.Mock<IPluginLoadContext>();
            var pluginDependencyContext = this.Mock<IPluginDependencyContext>();
            var sut = new Prise.DefaultAssemblyLoadStrategy(pluginLogger, pluginLoadContext, pluginDependencyContext);
            var loadFromDependencyContext = CreateLookupFunction<string, string>((c, a) => ValueOrProceed<string>.Proceed());
            var loadFromRemote = CreateLookupFunction<string, string>((c, a) => ValueOrProceed<string>.Proceed());
            var loadFromAppDomain = CreateLookupFunction<string, IntPtr>((c, a) => ValueOrProceed<IntPtr>.FromValue(new IntPtr(100), false));

            // Act
            var result = sut.LoadUnmanagedDll(someAssemblyName.Name, loadFromDependencyContext, loadFromRemote, loadFromAppDomain);

            // Assert
            Assert.Null(result.Path);
            Assert.Equal(new IntPtr(100), result.Pointer);
        }

        [Fact]
        public void Returns_AssemblyPath_FromRemote()
        {
            // Arrange
            var someAssembly = GetRealAssembly();
            var someAssemblyName = someAssembly.GetName();
            var pluginLogger = this.LooseMock<IPluginLogger>();
            var pluginLoadContext = this.Mock<IPluginLoadContext>();
            var pluginDependencyContext = this.Mock<IPluginDependencyContext>();
            var sut = new Prise.DefaultAssemblyLoadStrategy(pluginLogger, pluginLoadContext, pluginDependencyContext);
            var loadFromDependencyContext = CreateLookupFunction<string, string>((c, a) => ValueOrProceed<string>.Proceed());
            var loadFromRemote = CreateLookupFunction<string, string>((c, a) => ValueOrProceed<string>.FromValue(someAssemblyName.Name, false));
            var loadFromAppDomain = CreateLookupFunction<string, IntPtr>((c, a) => ValueOrProceed<IntPtr>.Proceed());

            // Act
            var result = sut.LoadUnmanagedDll(someAssemblyName.Name, loadFromDependencyContext, loadFromRemote, loadFromAppDomain);

            // Assert
            Assert.Equal(someAssemblyName.Name, result.Path);
            Assert.Equal(IntPtr.Zero, result.Pointer);
        }
    }
}
