using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Prise.Infrastructure;
using Xunit;

namespace Prise.Tests.DefaultAssemblyLoadStrategy
{
    public class LoadAssembly : TestBase
    {
        [Fact]
        public void Returns_Null_For_Empty_AssemblyName()
        {
            // Arrange
            var pluginLoadContext = this.Mock<IPluginLoadContext>();
            var pluginDependencyContext = this.Mock<IPluginDependencyContext>();
            var sut = new Prise.DefaultAssemblyLoadStrategy(pluginLoadContext, pluginDependencyContext);
            var emptyAssemblyname = new AssemblyName();

            // Act, Assert
            Assert.Null(sut.LoadAssembly(emptyAssemblyname, null, null, null));
        }

        [Fact]
        public void Returns_Null_When_AssemblyName_NotFound()
        {
            // Arrange
            var pluginLoadContext = this.Mock<IPluginLoadContext>();
            var pluginDependencyContext = this.Mock<IPluginDependencyContext>();
            var sut = new Prise.DefaultAssemblyLoadStrategy(pluginLoadContext, pluginDependencyContext);
            var assemblyname = new AssemblyName(this.CreateFixture<string>());
            var loadFromDependencyContext = CreateLookupFunction((c, a) => ValueOrProceed<Assembly>.Proceed());
            var loadFromRemote = CreateLookupFunction((c, a) => ValueOrProceed<Assembly>.Proceed());
            var loadFromAppDomain = CreateLookupFunction((c, a) => ValueOrProceed<Assembly>.Proceed());
            this.Arrange<IPluginDependencyContext>()
                .Setup(p => p.HostDependencies).Returns(Enumerable.Empty<HostDependency>());

            // Act, Assert
            Assert.Null(sut.LoadAssembly(assemblyname, loadFromDependencyContext, loadFromRemote, loadFromAppDomain));
        }

        private Func<IPluginLoadContext, AssemblyName, ValueOrProceed<Assembly>> CreateLookupFunction(Func<IPluginLoadContext, AssemblyName, ValueOrProceed<Assembly>> func) => func;
    }
}
